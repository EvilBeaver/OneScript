/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Language.LexicalAnalysis
{
    public struct Lexem
    {
        public LexemType Type;
        public string Content;
        public Token Token;
        public CodeRange Location { get; set; }
        
        public int LineNumber => Location.LineNumber;

        public static Lexem Empty()
        {
            return new Lexem() { Type = LexemType.NotALexem };
        }

        public static Lexem EndOfText()
        {
            return new Lexem() { Type = LexemType.EndOfText, Token = Token.EndOfText };
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Enum.GetName(typeof(LexemType), this.Type), Content);
        }
    }
}
