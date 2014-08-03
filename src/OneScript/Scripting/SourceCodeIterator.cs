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
            _startPosition = Lexer.OUT_OF_TEXT;
            _currentSymbol = '\0';

            if (!String.IsNullOrEmpty(code))
            {
                _lineCounter = 1;
                _lineBounds.Add(0);
            }
            else
            {
                _lineCounter = Lexer.OUT_OF_TEXT;
            }

        }

        public int Position
        {
            get { return _index; }
        }

        public int CurrentLine 
        { 
            get
            {
                return _lineCounter;
            }
        }
        
        public int CurrentColumn 
        {
            get
            {
                if (_startPosition == Lexer.OUT_OF_TEXT)
                {
                    return Lexer.OUT_OF_TEXT;
                }

                int start = GetLineBound(CurrentLine);
                return _index - start + 1;
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
                if (_currentSymbol == '\n')
                {
                    _lineCounter++;
                    if (_index < _code.Length)
                        _lineBounds.Add(_index + 1);
                }

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
            if(MoveNext())
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
            if (_index == Lexer.OUT_OF_TEXT && !MoveNext())
            {
                return false;
            }

            while (Char.IsWhiteSpace(_currentSymbol))
            {
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

        public string LineOfCode(int lineNumber)
        {
            int start = GetLineBound(lineNumber);
            int end = _code.IndexOf('\n', start);
            if (end >= 0)
            {
                return _code.Substring(start, end - start);
            }
            else
            {
                return _code.Substring(start);
            }
        }

        private int GetLineBound(int lineNumber)
        {
            return _lineBounds[lineNumber - 1];
        }

        public string GetContents()
        {
            return GetContents(0, 0);
        }

        public string GetContents(int padLeft, int padRight)
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
                return "";
            }

            var contents = _code.Substring(_startPosition + padLeft, len - padRight);
            
            _startPosition = _index + 1;

            return contents;
        }

        internal CodePositionInfo GetPositionInfo()
        {
            var posInfo = new CodePositionInfo()
            {
                LineNumber = CurrentLine,
                ColumnNumber = CurrentColumn,
                Code = LineOfCode(CurrentLine)
            };

            return posInfo;
        }
    }
}
