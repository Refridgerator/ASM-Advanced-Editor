﻿using System;
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
    [UserVisible(true)]
    internal class HighlightWordFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordFormatDefinition()
        {
            this.BackgroundColor = Colors.LightBlue;
            this.DisplayName = "Asm Highlight Word";
            this.ZOrder = 5;
        }
    }
}
