/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.SyntaxAnalysis;

namespace OneScript.Language.LexicalAnalysis
{
    public class DefaultLexer : PreprocessingLexer, ILexemGenerator
    {
        private string _code;

        public override Lexem NextLexem()
        {
            Lexem lex;
            while((lex = base.NextLexem()).Type == LexemType.Comment)
                ; // skip

            return lex;
        }
        
        public int CurrentColumn => Iterator.CurrentColumn;

        public int CurrentLine => Iterator.CurrentLine;

        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                Iterator = new SourceCodeIterator(value);
            }
        }
    }
}