using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class Lexer
    {
        private string _code;
        private SourceCodeIterator _iterator;

        public Lexer()
        {
            _iterator = new SourceCodeIterator(null);
        }

        public int Position
        {
            get
            {
                return _iterator.Position;
            }
        }

        public int CurrentLine
        {
            get
            {
                return _iterator.CurrentLine;
            }
        }
        public string Code 
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
                _iterator = new SourceCodeIterator(value);
            }
        }

        public Lexem NextLexem()
        {
            //_state = _emptyState;

            //while (true)
            //{
            //    if (_iterator.MoveToContent())
            //    {
            //        char cs = _iterator.CurrentSymbol;
            //        if (Char.IsLetter(cs) || cs == '_')
            //        {
            //            _state = _wordState;
            //        }
            //        else if (Char.IsDigit(cs))
            //        {
            //            _state = _numberState;
            //        }
            //        else if (cs == SpecialChars.DateQuote)
            //        {
            //            _state = _dateState;
            //        }
            //        else if (cs == SpecialChars.StringQuote)
            //        {
            //            _state = _stringState;
            //        }
            //        else if (SpecialChars.IsOperatorChar(cs))
            //        {
            //            _state = _operatorState;
            //        }
            //        else if (cs == SpecialChars.EndOperator)
            //        {
            //            _iterator.MoveNext();
            //            return new Lexem() { Type = LexemType.EndOperator, Token = Token.Semicolon };
            //        }
            //        else if (cs == '?')
            //        {
            //            _iterator.MoveNext();
            //            return new Lexem() { Type = LexemType.Identifier, Token = Token.Question };
            //        }
            //        else
            //        {
            //            var cp = _iterator.GetPositionInfo(_iterator.CurrentLine);
            //            throw new ParserException(cp, string.Format("Неизвестный символ {0}", cs));
            //        }

            //        var lex = _state.ReadNextLexem(_iterator);
            //        if (lex.Type == LexemType.NotALexem)
            //        {
            //            _state = _emptyState;
            //            continue;
            //        }

            //        return lex;

            //    }
            //    else
            //    {
            //        return Lexem.EndOfText();
            //    }
            //}
            _iterator.MoveToContent();
            return Lexem.Empty();
        }

        public const int OUT_OF_TEXT = -1;
    }
}
