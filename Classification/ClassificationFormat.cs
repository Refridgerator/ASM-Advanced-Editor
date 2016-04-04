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
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class AsmInstruction : ClassificationFormatDefinition
    {
        public AsmInstruction()
        {
            this.DisplayName = "Asm Instruction"; //human readable version of the name
            this.ForegroundColor = Colors.BlueViolet;
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "INSTRUCTION_FPU")]
    [Name("INSTRUCTION_FPU")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class AsmFpuInstruction : ClassificationFormatDefinition
    {
        public AsmFpuInstruction()
        {
            this.DisplayName = "Asm Fpu Instruction"; //human readable version of the name
            this.ForegroundColor = Colors.BlueViolet;
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "INSTRUCTION_SIMD")]
    [Name("INSTRUCTION_SIMD")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class AsmSimdInstruction : ClassificationFormatDefinition
    {
        public AsmSimdInstruction()
        {
            this.DisplayName = "Asm Simd Instruction"; //human readable version of the name
            this.ForegroundColor = Colors.BlueViolet;
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "REGISTER")]
    [Name("REGISTER")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class AsmRegister : ClassificationFormatDefinition
    {
        public AsmRegister()
        {
            this.DisplayName = "Asm Register"; //human readable version of the name
            this.ForegroundColor = Colors.Green;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "REGISTER_FPU")]
    [Name("REGISTER_FPU")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class AsmFpuRegister : ClassificationFormatDefinition
    {
        public AsmFpuRegister()
        {
            this.DisplayName = "Asm Fpu Register"; //human readable version of the name
            this.ForegroundColor = Colors.Green;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "REGISTER_SIMD")]
    [Name("REGISTER_SIMD")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class AsmSimdRegister : ClassificationFormatDefinition
    {
        public AsmSimdRegister()
        {
            this.DisplayName = "Asm Simd Register"; //human readable version of the name
            this.ForegroundColor = Colors.Green;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "DIRECTIVE")]
    [Name("DIRECTIVE")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class AsmDirective : ClassificationFormatDefinition
    {
        public AsmDirective()
        {
            this.DisplayName = "Asm Directive"; //human readable version of the name
            this.ForegroundColor = Colors.Purple;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "COMMENT_LINE")]
    [Name("COMMENT_LINE")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class AsmComment : ClassificationFormatDefinition
    {
        public AsmComment()
        {
            this.DisplayName = "Asm Comment"; //human readable version of the name
            this.ForegroundColor = Colors.Gray;
        }
    }



    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ASMNUMBER")]
    [Name("ASMNUMBER")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class AsmNumber : ClassificationFormatDefinition
    {
        public AsmNumber()
        {
            this.DisplayName = "Asm Number"; //human readable version of the name
            this.ForegroundColor = Colors.DarkRed;
        }
    }


    #endregion //Format definition
}
