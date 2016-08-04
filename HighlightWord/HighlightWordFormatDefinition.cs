using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace AsmAE
{
    [Export(typeof(EditorFormatDefinition))]
    [Name("MarkerFormatDefinition/AsmHighlightWord")]
    //[Name("ASMHIGHLIGHT")]
    [UserVisible(true)]
    internal class HighlightWordFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordFormatDefinition()
        {
            this.BackgroundColor = (Color)ColorConverter.ConvertFromString("#401D4E");
            this.DisplayName = "Asm Highlight Word";
            this.ZOrder = 5;
        }
    }
}
