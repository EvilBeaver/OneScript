/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("ЧтениеТекста", "TextReader")]
    class TextReadImpl : AutoContext<TextReadImpl>, IDisposable
    {
        StreamReader _reader;
        string _lineDelimiter = "\n";
        string _eolDelimiter = System.Environment.NewLine;
        Queue<char> _queue = new Queue<char>();

        [ContextMethod("Открыть", "Open")]
        public void Open(string path, IValue encoding = null, string lineDelimiter = "\n", string eolDelimiter = null)
        {
            if (encoding == null)
            {
                _reader = Environment.FileOpener.OpenReader(path);
            }
            else
            {
                var enc = TextEncodingEnum.GetEncoding(encoding);
                _reader = Environment.FileOpener.OpenReader(path, enc);
            }
            _lineDelimiter = lineDelimiter;
            _eolDelimiter = eolDelimiter ?? System.Environment.NewLine;
        }

        private void UpdateCharQueue()
        {
            // TODO: продумать оптимальный размер буфера
            int NEEDED_BUFFER_SIZE = 256;

            if (_reader.EndOfStream)
                return;

            if (_queue.Count < _eolDelimiter.Length) {
                while (_queue.Count < NEEDED_BUFFER_SIZE) {
                    var ic = _reader.Read ();
                    if (ic == -1)
                        break;
                    _queue.Enqueue ((char)ic);
                }
            }
        }

        private int ReadNext()
        {
            UpdateCharQueue();

            if (_queue.Count == 0)
                return -1;

            var currentChar = _queue.Dequeue();
            if (currentChar != _eolDelimiter[0])
                return currentChar;

            bool isEndOfLine = true;
            if (_eolDelimiter.Length > 1) {
                int eolIndex = 1;
                foreach (var c in _queue) {
                    if (_eolDelimiter [eolIndex] != c) {
                        isEndOfLine = false;
                        break;
                    }
                    ++eolIndex;
                    if (eolIndex == _eolDelimiter.Length)
                        break;
                }
            }

            if (!isEndOfLine)
                return currentChar;

            for (int i = 1; i < _eolDelimiter.Length; i++)
                _queue.Dequeue ();

            return -2; // TODO: Magic number!
        }

        [ContextMethod("Прочитать", "Read")]
        public IValue ReadAll(int size = 0)
        {
            RequireOpen();
            if (_reader.EndOfStream)
                return ValueFactory.Create();

            var sb = new StringBuilder ();
            var read = 0;
            do {
                var ic = ReadNext ();
                if (ic == -1)
                    break;
                if (ic == -2) {
                    sb.Append (_lineDelimiter);
                    read += _lineDelimiter.Length;
                } else {
                    sb.Append ((Char)ic);
                    ++read;
                }
            } while (size == 0 || read < size);
            return ValueFactory.Create (sb.ToString());
        }

        [ContextMethod("ПрочитатьСтроку", "ReadLine")]
        public IValue ReadLine()
        {
            RequireOpen();
            if (_reader.EndOfStream)
                return ValueFactory.Create();

            return ValueFactory.Create(_reader.ReadLine());
        }

        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            Dispose();
        }

        private void RequireOpen()
        {
            if (_reader == null)
            {
                throw new RuntimeException("Файл не открыт");
            }
        }

        [ScriptConstructor (Name = "По имени файла и кодировке")]
        public static IRuntimeContextInstance Constructor (IValue path, IValue encoding = null, IValue lineDelimiter = null, IValue eolDelimiter = null)
        {
            var reader = new TextReadImpl();
            reader.Open(path.AsString(), encoding, lineDelimiter?.ToString() ?? "\n", eolDelimiter ?.ToString());
            return reader;
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            var reader = new TextReadImpl();
            return reader;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        #endregion
    }
}
