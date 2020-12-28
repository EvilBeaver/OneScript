/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language.SyntaxAnalysis;

namespace OneScript.Language.LexicalAnalysis
{
    public class ExpressionBasedLexer : ILexer
    {
        private readonly Func<char, LexerState> _selector;
        private string _code;

        internal ExpressionBasedLexer(Func<char, LexerState> selector)
        {
            _selector = selector;
        }
        
        public Lexem NextLexem()
        {
            if (Iterator.MoveToContent())
            {
                var state = _selector(Iterator.CurrentSymbol);
                if (state == default)
                {
                    throw new SyntaxErrorException(Iterator.GetErrorPosition(),
                        LocalizedErrors.UnexpectedSymbol(Iterator.CurrentSymbol));
                }
                
                return state.ReadNextLexem(Iterator);
            }
            return Lexem.EndOfText();
        }

        public SourceCodeIterator Iterator { get; set; }
    }
}