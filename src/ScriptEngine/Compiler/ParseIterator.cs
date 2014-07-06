using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.Compiler
{
    class ParseIterator
    {
        private int _index;
        private int _startPosition;
        private char _currentSymbol;
        private string _code;
        private int _lineCounter = 1;
        private List<int> _lineBounds;

        public ParseIterator(string code)
        {
            _code = code;
            _index = 0;
            _startPosition = 0;
            _lineBounds = new List<int>();
            _lineBounds.Add(0);// first line

            if (_code.Length > 0)
            {
                _currentSymbol = _code[0];
            }
            else
                _currentSymbol = '\0';
        }

        public char CurrentSymbol
        {
            get
            {
                return _currentSymbol;
            }
        }

        public Word GetContents()
        {
            return GetContents(0, 0);
        }

        public int CurrentLine
        {
            get
            {
                return _lineCounter;
            }
        }

        public int GetLineBound(int lineNumber)
        {
            return _lineBounds[lineNumber-1];
        }

        public Word GetContents(int padLeft, int padRight)
        {
            int len;

            if (_startPosition == _index && _startPosition < _code.Length)
            {
                len = 1;
            }
            else if (_startPosition < _index)
            {
                len = _index - _startPosition;
            }
            else
            {
                return new Word() { start = -1 };
            }

            var contents = _code.Substring(_startPosition + padLeft, len - padRight);
            var word = new Word() { start = _startPosition, content = contents };

            _startPosition = _index + 1;

            return word;
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
                return false;
            }
        }

        public bool MoveBack()
        {
            _index--;
            if (_index >= 0)
            {
                _currentSymbol = _code[_index];
                if (_currentSymbol == '\n')
                {
                    _lineCounter--;
                }
                return true;
            }
            else
            {
                return false;
            }
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
            while (Char.IsWhiteSpace(_currentSymbol))
            {
                if (_currentSymbol == '\n')
                {
                    _lineCounter++;
                    if(_index < _code.Length)
                        _lineBounds.Add(_index+1);
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