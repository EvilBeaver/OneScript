using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Language
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
            lex = new Lexem()
            {
                Type = LexemType.Operator,
                Content = content,
                Token = LanguageDef.GetToken(content)
            };
            return lex;
        }
    }
}
