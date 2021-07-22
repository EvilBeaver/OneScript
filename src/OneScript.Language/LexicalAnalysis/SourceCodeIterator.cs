/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Sources;

namespace OneScript.Language.LexicalAnalysis
{
    public class SourceCodeIterator : ISourceCodeIndexer
    {
        private string _code;
        private char _currentSymbol;
        private int _lineCounter;
        private int _index;
        private int _startPosition;

        private List<int> _lineBounds;
        private bool _onNewLine;

        public bool OnNewLine { get; private set; }
        
        public bool StayOnSameLine { get; set; }

        private const int OUT_OF_TEXT = -1;

        public SourceCodeIterator(SourceCode code)
        {
            Source = code;
            InitOnString(code.GetSourceCode());
        }

        internal SourceCodeIterator()
        {
            InitOnString(String.Empty);
        }
        
        public SourceCode Source { get; }

        private void InitOnString(string code)
        {
            _code = code;
            int cap = code.Length < 512 ? 32 : 512;
            _lineBounds = new List<int>(cap);
            _index = OUT_OF_TEXT;
            _startPosition = OUT_OF_TEXT;
            _currentSymbol = '\0';
            _onNewLine = true;
            OnNewLine = true;

            if (!String.IsNullOrEmpty(code))
            {
                _lineCounter = 1;
                _lineBounds.Add(0);
            }
            else
            {
                _lineCounter = OUT_OF_TEXT;
            }
        }

        public int Position => _index;

        public int CurrentLine => _lineCounter;

        public int CurrentColumn
        {
            get
            {
                if (_startPosition == OUT_OF_TEXT)
                {
                    return OUT_OF_TEXT;
                }

                int start = GetLineBound(CurrentLine);
                return _index - start + 1;
            }
        }

        public char CurrentSymbol => _currentSymbol;


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
            char result = '\0';
            if (_index + 1 < _code.Length)
            {
                result = _code[_index + 1];
            }

            return result;

        }

        public bool MoveToContent()
        {
            if (SkipSpaces())
            {
                _startPosition = _index;
                OnNewLine = _onNewLine;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SkipSpaces()
        {
            if (_index == OUT_OF_TEXT && !MoveNext())
            {
                return false;
            }

            while (Char.IsWhiteSpace(_currentSymbol))
            {
                if (_currentSymbol == '\n')
                {
                    if (StayOnSameLine)
                        return false;
                    
                    _onNewLine = true;
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

        public string ReadToLineEnd()
        {
            while (MoveNext())
            {
                if (_currentSymbol == '\n')
                    break;
            }

            var res = GetContents();

            _onNewLine = true;
            OnNewLine = true;

            return res.Trim();
        }

        public string GetCodeLine(int lineNumber)
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
        
        public ReadOnlyMemory<char> GetContentSpan()
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
                return ReadOnlyMemory<char>.Empty;
            }

            var contents = _code.AsMemory(_startPosition, len);

            _startPosition = _index + 1;
            
            OnNewLine = _onNewLine;
            _onNewLine = false;

            return contents;
        }
        
        public string GetContents()
        {
            return GetContentSpan().ToString();
        }
    }
}
