using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    interface ILexemExtractor
    {
        public Lexem LastExtractedLexem { get; }
        public void NextLexem();
    }
}
