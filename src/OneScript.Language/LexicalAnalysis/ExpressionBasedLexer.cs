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
    public delegate LexerState LexerStateSelector(SourceCodeIterator iterator, char symbol);
    
    public class ExpressionBasedLexer : ILexer
    {
        private readonly LexerStateSelector _selector;

        internal ExpressionBasedLexer(LexerStateSelector selector)
        {
            _selector = selector;
        }
        
        public Lexem NextLexem()
        {
            if (Iterator.MoveToContent())
            {
                var state = _selector(Iterator, Iterator.CurrentSymbol);
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