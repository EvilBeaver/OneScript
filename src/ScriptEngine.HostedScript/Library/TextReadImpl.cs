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
    /// <summary>
    /// Предназначен для последовательного чтения файлов, в том числе большого размера.
    /// </summary>
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

        /// <summary>
        /// Открывает текстовый файл для чтения. Ранее открытый файл закрывается. 
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="encoding">Кодировка</param>
        /// <param name="lineDelimiter">Раздедитель строк</param>
        /// <param name="eolDelimiter">Разделитель строк в файле</param>
        /// <param name="monopoly">Открывать монопольно</param>
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

        /// <summary>
        /// Считывает строку указанной длины или до конца файла.
        /// </summary>
        /// <param name="size">Размер строки. Если не задан, текст считывается до конца файла</param>
        /// <returns>Строка - считанная строка, Неопределено - в файле больше нет данных</returns>
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

        /// <summary>
        /// Считывает очередную строку текстового файла.
        /// </summary>
        /// <param name="overridenLineDelimiter">Подстрока, считающаяся концом строки. Переопределяет РазделительСтрок, 
        /// переданный в конструктор или в метод Открыть</param>
        /// <returns>Строка - в случае успешного чтения, Неопределено - больше нет данных</returns>
        [ContextMethod("ПрочитатьСтроку", "ReadLine")]
        public IValue ReadLine(string overridenLineDelimiter = null)
        {
            RequireOpen();
            string l = _reader.ReadLine (overridenLineDelimiter ?? _lineDelimiter);

            if (l == null)
                return ValueFactory.Create();

            return ValueFactory.Create(l);
        }

        /// <summary>
        /// Закрывает открытый текстовый файл. Если файл был открыт монопольно, то после закрытия он становится доступен.
        /// </summary>
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

        /// <summary>
        /// Открывает текстовый файл для чтения.
        /// </summary>
        /// <param name="path">Строка. Путь к файлу</param>
        /// <returns>ЧтениеТекста</returns>
        [ScriptConstructor(Name = "По имени файла без кодировки")]
        public static TextReadImpl Constructor (IValue path)
        {
            var reader = new TextReadImpl ();
            reader.AnalyzeDefaultLineFeed = false;
            reader.Open (path.AsString (), null, "\n", "\r\n");
            return reader;
        }

        /// <summary>
        /// Открывает текстовый файл для чтения. Работает аналогично методу Открыть.
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="encoding">Кодировка файла</param>
        /// <param name="lineDelimiter">Разделитель строк</param>
        /// <param name="eolDelimiter">Разделитель строк в файле</param>
        /// <param name="monopoly">Открывать файл монопольно</param>
        /// <returns>ЧтениеТекста</returns>
        [ScriptConstructor(Name = "По имени файла")]
        public static TextReadImpl Constructor(IValue path, IValue encoding = null,
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

        /// <summary>
        /// Создаёт неинициализированный объект. Для инициализации необходимо открыть файл методом Открыть.
        /// </summary>
        /// <returns>ЧтениеТекста</returns>
        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static TextReadImpl Constructor()
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
