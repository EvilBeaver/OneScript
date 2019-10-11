/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Language.LexicalAnalysis
{
    public class PreprocessorUnknownTokenEventArgs : EventArgs
    {
        public bool IsHandled { get; set; }
        public ILexemGenerator Lexer { get; set; }
        public Lexem Lexem { get; set; }
    }
}