using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class SourceCodeIterator
    {
        private string _code;
        private char _currentSymbol;
        private int _lineCounter;
        private int _index;
        private int _startPosition;
        private List<int> _lineBounds;

        public SourceCodeIterator(string code)
        {
            _code = code;
            _lineBounds = new List<int>();
            _index = Lexer.OUT_OF_TEXT;

            if(!String.IsNullOrEmpty(code))
            {
                _index = 0;
                _startPosition = 0;
                _currentSymbol = code[_index];
                _lineCounter = 1;
                _lineBounds.Add(0);
            }
            else
            {
                _lineCounter = Lexer.OUT_OF_TEXT;
                _startPosition = Lexer.OUT_OF_TEXT;
                _currentSymbol = '\0';
            }

        }

        public int CurrentLine 
        { 
            get
            {
                return _lineCounter;
            }
        }

        public char CurrentSymbol 
        { 
            get
            {
                return _currentSymbol;
            }
        }

        public bool MoveNext()
        {
            _index++;
            if (_index < _code.Length)
            {
                _currentSymbol = _code[_index];
                return true;
            }
            else
            {
                _currentSymbol = '\0';
                return false;
            }
        }

        public char PeekNext()
        {
            var index = _index;
            var currentSymbol = _currentSymbol;
            char result = '\0';
            if(MoveNext() && SkipSpacesInternal(true))
            {
                result = _currentSymbol;
            }

            _currentSymbol = currentSymbol;
            _index = index;

            return result;

        }

        public bool MoveToContent()
        {
            if (SkipSpaces())
            {
                _startPosition = _index;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SkipSpaces()
        {
            return SkipSpacesInternal(false);
        }

        private bool SkipSpacesInternal(bool peeking)
        {
            while (Char.IsWhiteSpace(_currentSymbol))
            {
                if (_currentSymbol == '\n' && !peeking)
                {
                    _lineCounter++;
                    if (_index < _code.Length)
                        _lineBounds.Add(_index + 1);
                }

                if (!MoveNext())
                {
                    return false;
                }
            }

            if (_index >= _code.Length)
            {
                return false;
            }

            return true;
        }
    }
}
