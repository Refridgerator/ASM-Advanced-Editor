using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace AsmAE
{
    #region Format definition

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "INSTRUCTION")]
    [Name("INSTRUCTION")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class AsmInstruction : ClassificationFormatDefinition
    {
        public AsmInstruction()
        {
            this.DisplayName = "Asm Instruction";
            //this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#00F1817A");
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#7A81F1");
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "INSTRUCTION_FPU")]
    [Name("INSTRUCTION_FPU")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class AsmFpuInstruction : ClassificationFormatDefinition
    {
        public AsmFpuInstruction()
        {
            this.DisplayName = "Asm Fpu Instruction";
            //this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#00D5B639");
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#39B6D5");
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "INSTRUCTION_SIMD")]
    [Name("INSTRUCTION_SIMD")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class AsmSimdInstruction : ClassificationFormatDefinition
    {
        public AsmSimdInstruction()
        {
            this.DisplayName = "Asm Simd Instruction";
            //this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#00FF8882");
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#8288FF");
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "REGISTER")]
    [Name("REGISTER")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class AsmRegister : ClassificationFormatDefinition
    {
        public AsmRegister()
        {
            this.DisplayName = "Asm Register";
            //this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#00009D00");
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#009D00");
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "REGISTER_FPU")]
    [Name("REGISTER_FPU")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class AsmFpuRegister : ClassificationFormatDefinition
    {
        public AsmFpuRegister()
        {
            this.DisplayName = "Asm Fpu Register";
            //this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#0000B300");
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#00B300");
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "REGISTER_SIMD")]
    [Name("REGISTER_SIMD")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class AsmSimdRegister : ClassificationFormatDefinition
    {
        public AsmSimdRegister()
        {
            this.DisplayName = "Asm Simd Register";
            //this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#003DBC83");
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#83BC3D");
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "DIRECTIVE")]
    [Name("DIRECTIVE")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class AsmDirective : ClassificationFormatDefinition
    {
        public AsmDirective()
        {
            this.DisplayName = "Asm Directive";
            //this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#00A800A8");
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#A800A8");
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "COMMENT_LINE")]
    [Name("COMMENT_LINE")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class AsmComment : ClassificationFormatDefinition
    {
        public AsmComment()
        {
            this.DisplayName = "Asm Comment";
            //this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#00808080");
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#808080");
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ASMNUMBER")]
    [Name("ASMNUMBER")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class AsmNumber : ClassificationFormatDefinition
    {
        public AsmNumber()
        {
            this.DisplayName = "Asm Number";
            //this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#000000CA");
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString("#CA0000");
            this.ForegroundCustomizable = true;
        }
    }


    #endregion 
}
