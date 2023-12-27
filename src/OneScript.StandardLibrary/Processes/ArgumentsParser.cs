/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace OneScript.StandardLibrary.Processes
{
    public class ArgumentsParser
    {
        private readonly string _input;
        private readonly List<char> _quotes;
        private readonly List<string> _arguments;
        private readonly StringBuilder _currentArgument;
        
        private char _currentQuote;
        private bool _isInQuotes;

        public ArgumentsParser(string input)
        {
            _input = input;
            _currentArgument = new StringBuilder();
            _arguments = new List<string>();
            _quotes = new List<char> { '\'', '"' };
        }
        
        public string[] GetArguments()
        {
            if (string.IsNullOrWhiteSpace(_input))
            {
                return Array.Empty<string>();
            }

            if (_arguments.Count > 0)
            {
                return _arguments.ToArray();
            }
            
            _currentArgument.Clear();
            
            foreach (var currentChar in _input)
            {
                if (char.IsWhiteSpace(currentChar) && !_isInQuotes)
                {
                    PushArgument();
                }
                else if (_quotes.Contains(currentChar))
                {
                    HandleQuotes(currentChar);
                }
                else
                {
                    PushChar(currentChar);
                }
            }

            PushArgument();
            
            return _arguments.ToArray();
        }

        private void HandleQuotes(char quoteChar)
        {
            if (!_isInQuotes)
            {
                _isInQuotes = true;
                _currentQuote = quoteChar;
            }
            else if (quoteChar == _currentQuote)
            {
                _isInQuotes = false;
                _currentQuote = default;
            }
            else
            {
                PushChar(quoteChar);
            }
        }

        private void PushChar(char ch)
        {
            _currentArgument.Append(ch);
        }

        private void PushArgument()
        {
            if (_currentArgument.Length == 0)
            {
                return;
            }
            
            _arguments.Add(_currentArgument.ToString());
            _currentArgument.Clear();
        }
    }
}