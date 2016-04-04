using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace AsmAE
{
    internal static class OrdinaryClassificationDefinition
    {
        #region Type definition

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("REGISTER")]
        internal static ClassificationTypeDefinition REGISTER = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("REGISTER_FPU")]
        internal static ClassificationTypeDefinition REGISTER_FPU = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("REGISTER_SIMD")]
        internal static ClassificationTypeDefinition REGISTER_SIMD = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("INSTRUCTION")]
        internal static ClassificationTypeDefinition INSTRUCTION = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("INSTRUCTION_FPU")]
        internal static ClassificationTypeDefinition INSTRUCTION_FPU = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("INSTRUCTION_SIMD")]
        internal static ClassificationTypeDefinition INSTRUCTION_SIMD = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("AVXPROCESSOR")]
        internal static ClassificationTypeDefinition AVXPROCESSOR = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("COMMENT_LINE")]
        internal static ClassificationTypeDefinition COMMENT_LINE = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("DIRECTIVE")]
        internal static ClassificationTypeDefinition DIRECTIVE = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ASMNUMBER")]
        internal static ClassificationTypeDefinition ASMNUMBER = null;


        #endregion
    }
}
