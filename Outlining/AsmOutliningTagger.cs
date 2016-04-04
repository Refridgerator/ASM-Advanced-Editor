using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;

namespace AsmAE
{
    internal sealed class AsmOutliningTagger : ITagger<IOutliningRegionTag>
    {
        class PartialRegion
        {
            public int StartLine { get; set; }
            public int StartOffset { get; set; }
            public int Level { get; set; }
            public PartialRegion PartialParent { get; set; }
            public string ellipsis = "...";    //the characters that are displayed when the region is collapsed 
            //public string hover_text = ""; //the contents of the tooltip for the collapsed span  
            public string end_text = "";
            public bool collapsed = false;
        }

        class Region : PartialRegion
        {
            public int EndLine { get; set; }
        }

        ITextBuffer buffer;
        ITextSnapshot snapshot;
        List<Region> regions;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public AsmOutliningTagger(ITextBuffer buffer)
        {
            this.buffer = buffer;
            this.snapshot = buffer.CurrentSnapshot;
            this.regions = new List<Region>();
            this.ReParse();
            this.buffer.Changed += BufferChanged;
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;
            List<Region> currentRegions = this.regions;
            ITextSnapshot currentSnapshot = this.snapshot;
            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(currentSnapshot, SpanTrackingMode.EdgeExclusive);
            int startLineNumber = entire.Start.GetContainingLine().LineNumber;
            int endLineNumber = entire.End.GetContainingLine().LineNumber;

            foreach (var region in currentRegions)
            {
                if (region.StartLine <= endLineNumber && region.EndLine >= startLineNumber)
                {
                    var startLine = currentSnapshot.GetLineFromLineNumber(region.StartLine);
                    var endLine = currentSnapshot.GetLineFromLineNumber(region.EndLine);

                    //the region starts at the beginning of the "[", and goes until the *end* of the line that contains the "]".
                    yield return new TagSpan<IOutliningRegionTag>(
                        new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End),
                        new OutliningRegionTag(region.collapsed, false, region.ellipsis, region.ellipsis));
                }
            }
        }

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // If this isn't the most up-to-date version of the buffer, then ignore it for now 
            // (we'll eventually get another change event). 
            if (e.After != buffer.CurrentSnapshot) return;
            this.ReParse();
        }

        void ReParse()
        {
            ITextSnapshot newSnapshot = buffer.CurrentSnapshot;
            List<Region> newRegions = new List<Region>();

            //keep the current (deepest) partial region, which will have 
            // references to any parent partial regions.
            PartialRegion currentRegion = null;

            foreach (var line in newSnapshot.Lines)
            {
                int regionStart = -1;
                string linetext = line.GetText();

                string ellipsis_ = "";
                string end_text_ = "";
                bool collapsed_ = false;

                regionStart = -1;
                int collapsed_comment_start = linetext.IndexOf(";[+]", StringComparison.Ordinal);
                int comment_start = linetext.IndexOf(";[", StringComparison.Ordinal);
                int proc_start = linetext.IndexOf("proc", StringComparison.OrdinalIgnoreCase);
                int macro_start = linetext.IndexOf("macro", StringComparison.OrdinalIgnoreCase);
                if (macro_start > -1)
                {
                    regionStart = 0;
                    end_text_ = "endm";
                    if (collapsed_comment_start > -1)
                    {
                        collapsed_ = true;
                        ellipsis_ = linetext.Substring(0, collapsed_comment_start).TrimEnd();
                    }
                    else
                        ellipsis_ = linetext.TrimEnd();
                }
                if (proc_start > -1)
                {
                    regionStart = 0;
                    end_text_ = "endp";
                    if (collapsed_comment_start > -1)
                    {
                        collapsed_ = true;
                        ellipsis_ = linetext.Substring(0, collapsed_comment_start).TrimEnd();
                    }
                    else
                        ellipsis_ = linetext.TrimEnd();
                }
                if (comment_start > -1 && collapsed_comment_start < 0)
                {
                    regionStart = comment_start;
                    if (linetext.IndexOf(";[+") > -1)
                    {
                        collapsed_ = true;
                        ellipsis_ = linetext.Substring(comment_start + 3, -3 + linetext.Length - comment_start).Trim();
                    }
                    else
                        ellipsis_ = linetext.Substring(comment_start + 2, -2 + linetext.Length - comment_start).Trim();
                    end_text_ = ";]";
                }
                if (ellipsis_ == "") ellipsis_ = "..."; 

                if (regionStart > -1)
                {
                    int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
                    int newLevel = currentLevel + 1;

                    //levels are the same and we have an existing region; 
                    //end the current region and start the next 
                    if (currentLevel == newLevel && currentRegion != null)
                    {
                        // ???                     
                        newRegions.Add(new Region()
                        {
                            Level = currentRegion.Level,
                            StartLine = currentRegion.StartLine,
                            StartOffset = currentRegion.StartOffset,
                            EndLine = line.LineNumber,
                            ellipsis = ellipsis_,
                            end_text = end_text_,
                            collapsed = collapsed_
                        });
                        currentRegion = new PartialRegion()
                        {
                            Level = newLevel,
                            StartLine = line.LineNumber,
                            StartOffset = regionStart,
                            PartialParent = currentRegion.PartialParent,
                            ellipsis = ellipsis_,
                            end_text = end_text_,
                            collapsed = collapsed_
                        };
                    }
                    //this is a new (sub)region 
                    else
                    {
                        currentRegion = new PartialRegion()
                        {
                            Level = newLevel,
                            StartLine = line.LineNumber,
                            StartOffset = regionStart,
                            PartialParent = currentRegion,
                            ellipsis = ellipsis_,
                            end_text = end_text_,
                            collapsed = collapsed_
                        };
                    }
                }
                //lines that contain ";]","endm","endp" denote the end of a region
                else
                {
                    int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
                    regionStart = (currentRegion == null) ? -1 :
                        linetext.IndexOf(currentRegion.end_text, StringComparison.OrdinalIgnoreCase);

                    if (regionStart > -1)
                    {
                        int closingLevel = currentLevel;

                        //the regions match 
                        if (currentRegion != null && currentLevel == closingLevel)
                        {
                            newRegions.Add(new Region()
                            {
                                Level = currentLevel,
                                StartLine = currentRegion.StartLine,
                                StartOffset = currentRegion.StartOffset,
                                EndLine = line.LineNumber,
                                ellipsis = currentRegion.ellipsis,
                                collapsed = currentRegion.collapsed
                            });
                            currentRegion = currentRegion.PartialParent;
                        }
                    }
                }
            }

            //determine the changed span, and send a changed event with the new spans
            List<Span> oldSpans =
                new List<Span>(this.regions.Select(r => AsSnapshotSpan(r, this.snapshot)
                    .TranslateTo(newSnapshot, SpanTrackingMode.EdgeExclusive)
                    .Span));
            List<Span> newSpans =
                    new List<Span>(newRegions.Select(r => AsSnapshotSpan(r, newSnapshot).Span));

            NormalizedSpanCollection oldSpanCollection = new NormalizedSpanCollection(oldSpans);
            NormalizedSpanCollection newSpanCollection = new NormalizedSpanCollection(newSpans);

            //the changed regions are regions that appear in one set or the other, but not both.
            NormalizedSpanCollection removed =
            NormalizedSpanCollection.Difference(oldSpanCollection, newSpanCollection);

            int changeStart = int.MaxValue;
            int changeEnd = -1;

            if (removed.Count > 0)
            {
                changeStart = removed[0].Start;
                changeEnd = removed[removed.Count - 1].End;
            }

            if (newSpans.Count > 0)
            {
                changeStart = Math.Min(changeStart, newSpans[0].Start);
                changeEnd = Math.Max(changeEnd, newSpans[newSpans.Count - 1].End);
            }

            this.snapshot = newSnapshot;
            this.regions = newRegions;

            if (changeStart <= changeEnd)
            {
                ITextSnapshot snap = this.snapshot;
                if (this.TagsChanged != null)
                    this.TagsChanged(this, new SnapshotSpanEventArgs(
                        new SnapshotSpan(this.snapshot, Span.FromBounds(changeStart, changeEnd))));
            }
        }

        static SnapshotSpan AsSnapshotSpan(Region region, ITextSnapshot snapshot)
        {
            var startLine = snapshot.GetLineFromLineNumber(region.StartLine);
            var endLine = (region.StartLine == region.EndLine) ?
                startLine : snapshot.GetLineFromLineNumber(region.EndLine);
            return new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End);
        }
    }
}
