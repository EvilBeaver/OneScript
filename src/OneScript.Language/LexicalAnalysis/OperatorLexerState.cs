/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public class OperatorLexerState : LexerState
    {
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            if(iterator.CurrentSymbol == '<')
            {
                char next = iterator.PeekNext();
                
                if (next == '>' || next == '=')
                {
                    iterator.MoveNext();
                    iterator.MoveNext();
                    return ExtractOperatorContent(iterator);
                }

            }
            else if(iterator.CurrentSymbol == '>')
            {
                char next = iterator.PeekNext();
                
                if (next == '=')
                {
                    iterator.MoveNext();
                    iterator.MoveNext();
                    return ExtractOperatorContent(iterator);
                }
                
            }

            var lex = ExtractOperatorContent(iterator);
            
            iterator.MoveNext();

            return lex;
            
        }

        private static Lexem ExtractOperatorContent(SourceCodeIterator iterator)
        {
            Lexem lex;
            var content = iterator.GetContents();
            lex = new Lexem
            {
                Type = LexemType.Operator,
                Content = content,
                Token = LanguageDef.GetToken(content),
                Location = new CodeRange(
                    iterator.CurrentLine,
                    iterator.CurrentColumn)
            };
            return lex;
        }
    }
}
