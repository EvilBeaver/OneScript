/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OneScript.StandardLibrary
{

    public class CustomLineFeedStreamReader : IDisposable
    {
        private TextReader _reader;
        private readonly string _eolDelimiter;
        private Queue<char> _buffer = new Queue<char> ();
        private bool _analyzeDefaults = false;

        public CustomLineFeedStreamReader (TextReader underlyingReader, string eolDelimiter, bool analyzeDefaults)
        {
            if(underlyingReader == null)
                throw new ArgumentNullException(nameof(underlyingReader));
            if(eolDelimiter == null)
                throw new ArgumentNullException(nameof(eolDelimiter));
            
            _reader = underlyingReader;
            _eolDelimiter = eolDelimiter;
            _analyzeDefaults = analyzeDefaults;
        }

        private void UpdateCharQueue (int minimalLentgh = 1)
        {
            while (_buffer.Count < minimalLentgh) {
                int ic = _reader.Read ();
                if (ic == -1)
                    break;
                _buffer.Enqueue ((char)ic);
            }
        }

        public int Read ()
        {
            if (_buffer.Count == 0)
                UpdateCharQueue ();

            if (_buffer.Count == 0)
                return -1;

            if (_analyzeDefaults && _buffer.Peek () == '\r') {

                _buffer.Dequeue ();
                UpdateCharQueue ();

                if (_buffer.Count > 0 && _buffer.Peek () == '\n') {
                    _buffer.Dequeue ();
                    UpdateCharQueue ();
                }

                return '\n';
            }

            if (_eolDelimiter.Length > 0 && _buffer.Peek() == _eolDelimiter [0]) {
                bool isEol = true;
                UpdateCharQueue (_eolDelimiter.Length);

                var eolIndex = 0;
                foreach (var bufChar in _buffer) {
                    
                    if (eolIndex >= _eolDelimiter.Length)
                        break;
                    
                    if (bufChar != _eolDelimiter [eolIndex]) {
                        isEol = false;
                        break;
                    }

                    ++eolIndex;
                }

                if (isEol) {
                    
                    foreach (var eolChar in _eolDelimiter)
                        _buffer.Dequeue ();
                    
                    return '\n';
                }
            }

            return _buffer.Dequeue ();
        }

        public string ReadUntil(string endOfString, out bool eosMet)
        {
            var sb = new StringBuilder ();
            eosMet = false;

            while (!eosMet) {
                var ic = Read ();
                if (ic == -1) {
                    break;
                }

                var c = (char)ic;

                sb.Append (c);

                if (endOfString.Length > 0 && c == endOfString [endOfString.Length - 1]) {
                    if (sb.Length >= endOfString.Length) {

                        var substring = sb.ToString (sb.Length - endOfString.Length, endOfString.Length);
                        if (substring.Equals (endOfString, StringComparison.InvariantCulture)) {
                            eosMet = true;
                            sb.Remove (sb.Length - endOfString.Length, endOfString.Length);
                        }

                    }
                }
            }

            if (sb.Length == 0 && !eosMet)
                return null;
            return sb.ToString ();
        }

        public string ReadLine (string lineDelimiter)
        {
            bool eol;
            var l = ReadUntil (lineDelimiter, out eol);
            return l;
        }

        public void Dispose ()
        {
            if (_reader != null) {
                _reader.Dispose ();
                _reader = null;
            }
        }
    }
    
}
