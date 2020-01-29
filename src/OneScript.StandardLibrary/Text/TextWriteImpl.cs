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

namespace OneScript.StandardLibrary.Text
{
    [ContextClass("ЗаписьТекста", "TextWriter")]
    public class TextWriteImpl : AutoContext<TextWriteImpl>, IDisposable
    {
        StreamWriter _writer;
        string _lineDelimiter = "";
        string _eolReplacement = "";

        public TextWriteImpl()
        {

        }

        public TextWriteImpl(string path, IValue encoding)
        {
            Open(path, encoding);
        }

        public TextWriteImpl(string path, IValue encoding, bool append)
        {
            Open(path, encoding, null, append);
        }

        /// <summary>
        /// Открывает файл для записи.
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="encoding">Кодировка (необязательный). По умолчанию используется utf-8</param>
        /// <param name="lineDelimiter">Разделитель строк (необязательный).</param>
        /// <param name="append">Признак добавления в конец файла (необязательный)</param>
        /// <param name="eolReplacement">Разделитель строк в файле (необязательный).</param>
        [ContextMethod("Открыть", "Open")]
        public void Open(string path, IValue encoding = null, string lineDelimiter = null, bool append = false, string eolReplacement = null)
        {
            _lineDelimiter = lineDelimiter ?? "\n";
            _eolReplacement = eolReplacement ?? "\r\n";

            Encoding enc;
            if (encoding == null)
            {
                enc = new UTF8Encoding(true);
            }
            else
            {
                enc = TextEncodingEnum.GetEncoding(encoding);
                if (enc.WebName == "utf-8" && append == true)
                    enc = new UTF8Encoding(false);
            }

            _writer = new StreamWriter(path, append, enc);
            _writer.AutoFlush = true;
        }

        [ContextMethod("Закрыть","Close")]
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Записывает текст "как есть"
        /// </summary>
        /// <param name="what">Текст для записи</param>
        [ContextMethod("Записать", "Write")]
        public void Write(string what)
        {
            ThrowIfNotOpened();

            var stringToOutput = what.Replace ("\n", _eolReplacement);
            
            _writer.Write(stringToOutput);
        }

        /// <summary>
        /// Записывает текст и добавляет перевод строки
        /// </summary>
        /// <param name="what">Текст для записи</param>
        /// <param name="delimiter">Разделитель строк</param>
        [ContextMethod("ЗаписатьСтроку", "WriteLine")]
        public void WriteLine(string what, IValue delimiter = null)
        {
            ThrowIfNotOpened();

            Write (what);

            var sDelimiter = _lineDelimiter;
            if (delimiter != null && delimiter.GetRawValue ().DataType != DataType.Undefined)
                sDelimiter = delimiter.GetRawValue ().AsString ();

            Write (sDelimiter);
        }

        public void ThrowIfNotOpened()
        {
            if (_writer == null)
                throw new RuntimeException("Файл не открыт");
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }
        }

        /// <summary>
        /// Создает объект с начальными значениями имени файла и кодировки.
        /// </summary>
        /// <param name="path">Имя файла</param>
        /// <param name="encoding">Кодировка в виде строки</param>
        /// <param name="lineDelimiter">Символ - разделитель строк</param>
        /// <param name="append">Признак добавления в конец файла (необязательный)</param>
        /// <param name="eolReplacement">Разделитель строк в файле (необязательный).</param>
        [ScriptConstructor(Name = "По имени файла")]
        public static TextWriteImpl Constructor(IValue path, IValue encoding = null, IValue lineDelimiter = null, IValue append = null, IValue eolReplacement = null)
        {
            bool isAppend = append != null && append.AsBoolean();
            var result = new TextWriteImpl ();

            string sLineDelimiter = lineDelimiter?.GetRawValue().AsString () ?? "\n";
            string sEolReplacement = eolReplacement?.GetRawValue().AsString () ?? "\r\n";

            result.Open (path.AsString (), encoding, sLineDelimiter, isAppend, sEolReplacement);

            return result;
        }

        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static TextWriteImpl Constructor()
        {
            return new TextWriteImpl();
        }

    }
}
