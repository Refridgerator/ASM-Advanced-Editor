using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace AsmAE
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("asm")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class AsmClassifierProvider : ITaggerProvider
    {

        [Export]
        [Name("asm")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition AsmContentType = null;

        [Export]
        [FileExtension(".asm")]
        [ContentType("asm")]
        internal static FileExtensionToContentTypeDefinition AsmFileType = null;

        [Export]
        [FileExtension(".inc")]
        [ContentType("asm")]
        internal static FileExtensionToContentTypeDefinition IncFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<AsmTokenTag> AsmTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<AsmTokenTag>(buffer);

            return new AsmClassifier(buffer, AsmTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class AsmClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<AsmTokenTag> _aggregator;
        IDictionary<AsmTokens, IClassificationType> _asmTypes;

        internal AsmClassifier(ITextBuffer buffer,
                               ITagAggregator<AsmTokenTag> AsmTagAggregator,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = AsmTagAggregator;
            _asmTypes = new Dictionary<AsmTokens, IClassificationType>();

            _asmTypes[AsmTokens.REGISTER] = typeService.GetClassificationType("REGISTER");
            _asmTypes[AsmTokens.REGISTER_FPU] = typeService.GetClassificationType("REGISTER_FPU");
            _asmTypes[AsmTokens.REGISTER_SIMD] = typeService.GetClassificationType("REGISTER_SIMD");
            _asmTypes[AsmTokens.INSTRUCTION] = typeService.GetClassificationType("INSTRUCTION");
            _asmTypes[AsmTokens.INSTRUCTION_FPU] = typeService.GetClassificationType("INSTRUCTION_FPU");
            _asmTypes[AsmTokens.INSTRUCTION_SIMD] = typeService.GetClassificationType("INSTRUCTION_SIMD");
            _asmTypes[AsmTokens.DIRECTIVE] = typeService.GetClassificationType("DIRECTIVE");
            _asmTypes[AsmTokens.COMMENT_LINE] = typeService.GetClassificationType("COMMENT_LINE");
            _asmTypes[AsmTokens.NUMBER] = typeService.GetClassificationType("ASMNUMBER");
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            ITextSnapshot snapshot = spans[0].Snapshot;
            foreach (var tagSpan in this._aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return new TagSpan<ClassificationTag>(tagSpans[0],
                    new ClassificationTag(_asmTypes[tagSpan.Tag.type]));
            }
        }
    }
}
