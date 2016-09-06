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
        class Region
        {
            public int StartLine = -1;
            public int StartOffset = -1;
            public string ellipsis = "...";
            //public string hover_text = "";
            public bool collapsed = false;
            public int EndLine = -1;
            public bool breaking = false;
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

                    yield return new TagSpan<IOutliningRegionTag>(
                        new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End),
                        new OutliningRegionTag(region.collapsed, false, region.ellipsis, region.ellipsis));
                }
            }
        }

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            if (e.After != buffer.CurrentSnapshot) return;
            this.ReParse();
        }

        void ReParse()
        {
            ITextSnapshot newSnapshot = buffer.CurrentSnapshot;
            List<Region> newRegions = new List<Region>();

            Region currentRegion = null;
            Region procRegion = null;
            Region macroRegion = null;

            foreach (var line in newSnapshot.Lines)
            {
                string linetext = line.GetText();

                string ellipsis_ = "";
                bool collapsed_ = false;

                int collapsed_outlining_start = linetext.IndexOf(";[+", StringComparison.Ordinal);
                int outlining_start = linetext.IndexOf(";[", StringComparison.Ordinal);
                int outlining_end = linetext.IndexOf(";]", StringComparison.Ordinal);
                int comment_start = linetext.IndexOf(";", StringComparison.Ordinal);
                int proc_start = linetext.IndexOf("proc", StringComparison.OrdinalIgnoreCase);
                int macro_start = linetext.IndexOf("macro", StringComparison.OrdinalIgnoreCase);
                int proc_end = linetext.IndexOf("endp", StringComparison.OrdinalIgnoreCase);
                int macro_end = linetext.IndexOf("endm", StringComparison.OrdinalIgnoreCase);
                if (comment_start > -1)
                {
                    if (comment_start < proc_start) proc_start = -1;
                    if (comment_start < macro_start) macro_start = -1;
                    if (comment_start < proc_end) proc_end = -1;
                    if (comment_start < macro_end) macro_end = -1;
                }
                #region ------- macro ------
                if (macro_start > -1)
                {
                    if (collapsed_outlining_start > -1)
                    {
                        collapsed_ = true;
                        ellipsis_ = linetext.Substring(0, collapsed_outlining_start).TrimEnd();
                    }
                    else
                        ellipsis_ = linetext.TrimEnd();
                    //
                    macroRegion = new Region()
                    {
                        StartLine = line.LineNumber,
                        StartOffset = 0,
                        ellipsis = ellipsis_,
                        collapsed = collapsed_,
                        EndLine = -1,
                        breaking = true
                    };
                    newRegions.Add(macroRegion);
                    continue;
                }
                if (macro_end > -1)
                {
                    if (macroRegion != null)
                        macroRegion.EndLine = line.LineNumber;
                    macroRegion = null;
                    continue;
                }
                #endregion
                #region ------- proc ------
                if (proc_start > -1)
                {
                    if (collapsed_outlining_start > -1)
                    {
                        collapsed_ = true;
                        ellipsis_ = linetext.Substring(0, collapsed_outlining_start).TrimEnd();
                    }
                    else
                        ellipsis_ = linetext.TrimEnd();
                    //
                    procRegion = new Region()
                    {
                        StartLine = line.LineNumber,
                        StartOffset = 0,
                        ellipsis = ellipsis_,
                        collapsed = collapsed_,
                        EndLine = -1,
                        breaking = true
                    };
                    newRegions.Add(procRegion);
                    continue;
                }
                if (proc_end > -1)
                {
                    if (procRegion != null)
                        procRegion.EndLine = line.LineNumber;
                    procRegion = null;
                    continue;
                }
                #endregion
                //------------------------------
                if (outlining_start > -1)
                {
                    if (collapsed_outlining_start > -1)
                    {
                        collapsed_ = true;
                        ellipsis_ = linetext.Substring(outlining_start + 3, -3 + linetext.Length - outlining_start).Trim();
                    }
                    else
                        ellipsis_ = linetext.Substring(outlining_start + 2, -2 + linetext.Length - outlining_start).Trim();

                    if (ellipsis_ == "") ellipsis_ = "...";
                    //
                    currentRegion = new Region()
                    {
                        StartLine = line.LineNumber,
                        StartOffset = outlining_start,
                        ellipsis = ellipsis_,
                        collapsed = collapsed_,
                        EndLine = -1
                    };
                    newRegions.Add(currentRegion);
                    continue;
                }
                //
                if (outlining_end > -1)
                {
					int i = newRegions.Count - 1;
					while (i >= 0)
					{
						Region region = newRegions[i];
						if (region.EndLine < 0 && region.breaking) break;
						if (region.EndLine < 0)
						{
							region.EndLine = line.LineNumber;
							break;
						}
						i--;
					}
				continue;
                }
            }
            // remove unclosed regions
            int j = newRegions.Count - 1;
            while (j >= 0)
            {
                Region region = newRegions[j];
                if (region.EndLine < 0)
                    newRegions.RemoveAt(j);
                j--;
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
