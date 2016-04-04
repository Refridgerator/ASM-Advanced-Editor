using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace AsmAE
{    
    [Export(typeof(IQuickInfoSourceProvider))]
    [ContentType("asm")]
    [Name("AsmQuickInfo")]
    class AsmQuickInfoSourceProvider : IQuickInfoSourceProvider
    {
        [Import]
        IBufferTagAggregatorFactoryService aggService = null;
        [Import]
        internal ITextSearchService2 TextSearchService { get; set; }

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new AsmQuickInfoSource(
                textBuffer, 
                aggService.CreateTagAggregator<AsmTokenTag>(textBuffer),
                TextSearchService
                );
        }
    }

    class AsmQuickInfoSource : IQuickInfoSource
    {
        private ITagAggregator<AsmTokenTag> _aggregator;
        private ITextBuffer _buffer;
        private ITextSearchService2 _textSearchService;
        private bool _disposed = false;
        private Dictionary<string, string> hints;

        public AsmQuickInfoSource(
            ITextBuffer buffer, 
            ITagAggregator<AsmTokenTag> aggregator,
            ITextSearchService2 textSearchService
            )
        {
            _aggregator = aggregator;
            _buffer = buffer;
            _textSearchService = textSearchService;

            // load hints
            hints = new Dictionary<string, string>();
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetName().Name + "." + "instruction_hints.ini";
            string last_section = "";
            string line;
            AsmTokens classification = AsmTokens.TEXT;
            using (Stream file = assembly.GetManifestResourceStream(resourceName))
            using (TextReader textReader = new StreamReader(file))
                while ((line = textReader.ReadLine()) != null)
                {
                    try
                    {
                        line = line.Trim();
                        if (line.IndexOf("[") > -1)
                        {
                            last_section = line;
                            continue;
                        }
                        if (line.StartsWith(";")) continue;
                        int pos = line.LastIndexOf("=");
                        if (pos < 1) continue;
                        string instruction = line.Substring(0, pos).Trim().ToLower();
                        string description = line.Substring(pos + 1, line.Length - pos - 1).Trim().Replace("\\n", "\n");

                        if (instruction.IndexOf("/") > 0)
                        {
                            string[] instructions = instruction.Split("/".ToCharArray());
                            string[] descriptions = description.Split("/".ToCharArray());

                            for (int i = 0; i < instructions.Length; i++)
                            {
                                if (!hints.ContainsKey(instructions[i]))
                                hints.Add(
                                    instructions[i].Trim(),
                                    descriptions[Math.Min(i, descriptions.Length - 1)].Trim()
                                    );
                                else
                                    Trace.WriteLine(string.Format("Warning: instruction {0} from section {1} already added.", instructions[i], last_section));
                            }
                        }
                        else
                            if (!hints.ContainsKey(instruction))
                                hints.Add(instruction, description);
                            else
                                Trace.WriteLine(string.Format("Warning: instruction {0} from section {1} already added.", instruction, last_section));
                    }
                    catch (Exception e)
                    {

                    }
                }
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;
            if (_disposed) throw new ObjectDisposedException("AsmQuickInfoSource");
            var currentSnapshot = _buffer.CurrentSnapshot;

            var triggerPoint = (SnapshotPoint) session.GetTriggerPoint(_buffer.CurrentSnapshot);
            if (triggerPoint == null) return;
            var tags = _aggregator.GetTags(new SnapshotSpan(triggerPoint, triggerPoint));
            foreach (IMappingTagSpan<AsmTokenTag> curTag in tags)
            {
                // instruction description
                if (curTag.Tag.type == AsmTokens.INSTRUCTION || curTag.Tag.type == AsmTokens.INSTRUCTION_FPU || curTag.Tag.type == AsmTokens.INSTRUCTION_SIMD)
                {
                    var tagSpan = curTag.Span.GetSpans(_buffer).First();
                    string instruction = tagSpan.GetText().ToLower();
                    if (hints.ContainsKey(instruction))
                        quickInfoContent.Add(hints[instruction]);
                    applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(tagSpan, SpanTrackingMode.EdgeExclusive);
                    
                    continue;
                }

                // dynamic hints
                if (curTag.Tag.type == AsmTokens.REGISTER || curTag.Tag.type == AsmTokens.REGISTER_SIMD)
                {
                    var tagSpan = curTag.Span.GetSpans(_buffer).First();
                    
                    string reg = tagSpan.GetText();
                    SnapshotPoint SnapshotPointTag = new SnapshotPoint(currentSnapshot, tagSpan.Start);
                    SnapshotSpan? procspan = _textSearchService.Find(SnapshotPointTag,
                        @"(?m)^\s*\w+\s+(proc|macro)",
                        FindOptions.SearchReverse | FindOptions.UseRegularExpressions);

                    SnapshotSpan? lineend = _textSearchService.Find(new SnapshotPoint(currentSnapshot, tagSpan.Start),
                         @"(?=\r?$)",
                         FindOptions.UseRegularExpressions);
                    if (lineend.HasValue)
                        SnapshotPointTag = lineend.Value.End;
                    SnapshotSpan? linecommentspan = _textSearchService.Find(SnapshotPointTag,
                           @"(?m)^\s*((\w+\s*" + reg + @"\s*,.+;=.+)|(.*;" + reg + @"=.+))(?=\r?$)",
                           FindOptions.SearchReverse | FindOptions.UseRegularExpressions);

                    if (!procspan.HasValue || !linecommentspan.HasValue) return;
                    if (linecommentspan.Value.Start < procspan.Value.Start) return;

                    string str = linecommentspan.Value.GetText();
                                        
                    applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(tagSpan, SpanTrackingMode.EdgeExclusive);
                    int pos = str.IndexOf(";=");
                    int pos2 = str.IndexOf(";" + reg + "=");                   
                    if (pos > pos2)
                        quickInfoContent.Add(str.Substring(pos + 2, -2 + str.Length - pos).Trim());
                    else
                        quickInfoContent.Add(str.Substring(pos2 + 2 + reg.Length, -2 - reg.Length + str.Length - pos2).Trim());
                }
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}

