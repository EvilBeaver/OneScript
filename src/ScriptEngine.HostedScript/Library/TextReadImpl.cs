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
            if (_reader.EndOfStream)
                return;

            var ic = _reader.Read();
            if (ic == -1)
                return;
            
            var c = (char)ic;
            if (c != _eolDelimiter[0]) {
                _queue.Enqueue(c);
                return;
            }

            // Если встретили первый символ разделителя строк,
            // то необходимо проверить, действительно ли встретили разделитель

            var tempQueue = new List<char>();
            tempQueue.Add(c);
            var isEol = true;
            for (int i = 1; i < _eolDelimiter.Length; i++) {
                
                ic = _reader.Read ();
                if (ic == -1) {
                    isEol = false;
                    break;
                }

                c = (char)ic;
                tempQueue.Add(c);
                if (c != _eolDelimiter[i]) {
                    isEol = false;
                    break;
                }
            }

            if (isEol) {
                // Для внутренней работы строк в качестве символа переноса всегда используем Символы.ПС
                _queue.Enqueue('\n');
            } else {
                foreach (var ch in tempQueue)
                    _queue.Enqueue(ch);
            }
        }

        private int ReadNext()
        {
            UpdateCharQueue();
            if (_queue.Count == 0)
                return -1;

            return _queue.Dequeue();
        }

        [ContextMethod("Прочитать", "Read")]
        public IValue ReadAll(int size = 0)
        {
            RequireOpen();
            UpdateCharQueue();
            if (_queue.Count == 0)
                return ValueFactory.Create();

            var sb = new StringBuilder();
            var read = 0;
            do {
                var ic = ReadNext();
                if (ic == -1)
                    break;
                if ((char)ic == '\n')
                    sb.Append(_lineDelimiter);
                else
                    sb.Append((char)ic);
                ++read;
            } while (size == 0 || read < size);

            return ValueFactory.Create(sb.ToString());
        }

        [ContextMethod("ПрочитатьСтроку", "ReadLine")]
        public IValue ReadLine()
        {
            RequireOpen();
            UpdateCharQueue();
            if (_queue.Count == 0)
                return ValueFactory.Create();

            var sb = new StringBuilder();
            do {
                var ic = ReadNext();

                if (ic == -1)
                    break;

                if ((char)ic == '\n')
                    break;
                
                sb.Append((char)ic);

            } while (true);

            return ValueFactory.Create(sb.ToString());
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

        [ScriptConstructor(Name = "По имени файла и кодировке")]
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
