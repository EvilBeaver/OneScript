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

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("ЧтениеТекста", "TextReader")]
    public class TextReadImpl : AutoContext<TextReadImpl>, IDisposable
    {
        // TextReader _reader;
        CustomLineFeedStreamReader _reader;
        string _lineDelimiter = "\n";

        public TextReadImpl ()
        {
            AnalyzeDefaultLineFeed = true;
        }

        [ContextMethod("Открыть", "Open")]
        public void Open(string path, IValue encoding = null, string lineDelimiter = "\n", string eolDelimiter = null,
            bool? monopoly = null)
        {
            Close();
            TextReader imReader;
            var shareMode = (monopoly ?? true) ? FileShare.None : FileShare.ReadWrite;
            if (encoding == null)
            {
                imReader = Environment.FileOpener.OpenReader(path, shareMode);
            }
            else
            {
                var enc = TextEncodingEnum.GetEncoding(encoding);
                imReader = Environment.FileOpener.OpenReader(path, shareMode, enc);
            }
            _lineDelimiter = lineDelimiter ?? "\n";
            if (eolDelimiter != null)
                _reader = new CustomLineFeedStreamReader(imReader, eolDelimiter, AnalyzeDefaultLineFeed);
            else
                _reader = new CustomLineFeedStreamReader(imReader, "\r\n", AnalyzeDefaultLineFeed);

        }

        private bool AnalyzeDefaultLineFeed { get; set; }

        private int ReadNext()
        {
            return _reader.Read ();
        }

        [ContextMethod("Прочитать", "Read")]
        public IValue ReadAll(int size = 0)
        {
            RequireOpen();

            var sb = new StringBuilder();
            var read = 0;
            do {
                var ic = ReadNext();
                if (ic == -1)
                    break;
                sb.Append((char)ic);
                ++read;
            } while (size == 0 || read < size);

            if (sb.Length == 0)
                return ValueFactory.Create ();
            
            return ValueFactory.Create(sb.ToString());
        }

        [ContextMethod("ПрочитатьСтроку", "ReadLine")]
        public IValue ReadLine(string overridenLineDelimiter = null)
        {
            RequireOpen();
            string l = _reader.ReadLine (overridenLineDelimiter ?? _lineDelimiter);

            if (l == null)
                return ValueFactory.Create();

            return ValueFactory.Create(l);
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

        [ScriptConstructor(Name = "По имени файла без кодировки")]
        public static IRuntimeContextInstance Constructor (IValue path)
        {
            var reader = new TextReadImpl ();
            reader.AnalyzeDefaultLineFeed = false;
            reader.Open (path.AsString (), null, "\n", "\r\n");
            return reader;
        }

        [ScriptConstructor(Name = "По имени файла")]
        public static IRuntimeContextInstance Constructor(IValue path, IValue encoding = null,
            IValue lineDelimiter = null, IValue eolDelimiter = null, IValue monopoly = null)
        {
            var reader = new TextReadImpl();
            if (lineDelimiter != null)
                reader.AnalyzeDefaultLineFeed = false;
            
            reader.Open(path.AsString(), encoding,
                lineDelimiter?.GetRawValue().AsString() ?? "\n",
                eolDelimiter?.GetRawValue().AsString(),
                monopoly?.AsBoolean() ?? true);
            
            return reader;
        }

        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static IRuntimeContextInstance Constructor()
        {
            var reader = new TextReadImpl();
            reader.AnalyzeDefaultLineFeed = false;
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
