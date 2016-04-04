using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsmAE
{
    public enum AsmTokens
    {
        TEXT,

        DIRECTIVE,
        COMMENT_LINE,
        NUMBER,

        REGISTER,
        REGISTER_FPU,
        REGISTER_SIMD,

        INSTRUCTION,
        INSTRUCTION_FPU,
        INSTRUCTION_SIMD
    }
}
