/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Language.LexicalAnalysis
{
    public static class LexerExtensions
    {
        public static int ReadToLineEnd(this ILexer lexer)
        {
            var data = lexer.Iterator.ReadToLineEnd();
            return data.Length;
        }

        public static Lexem NextLexemOnSameLine(this ILexer lexer)
        {
            if (lexer.Iterator.CurrentSymbol == '\n')
            {
                return Lexem.EndOfText();
            }
            
            var currentLine = lexer.Iterator.CurrentLine;
            if (lexer.Iterator.MoveToContent())
            {
                if (currentLine == lexer.Iterator.CurrentLine)
                    return lexer.NextLexem();
            }

            return Lexem.EndOfText();
        }

        public static void SkipTillLineEnd(this ILexer lexer)
        {
            lexer.ReadToLineEnd();
        }
        
        public static Lexem SkipTillNextStatement(this ILexer lexer)
        {
            throw new NotImplementedException();
        }
    }
}