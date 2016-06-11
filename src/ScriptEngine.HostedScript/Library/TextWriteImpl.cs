﻿/*----------------------------------------------------------
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
    [ContextClass("ЗаписьТекста", "TextWriter")]
    class TextWriteImpl : AutoContext<TextWriteImpl>, IDisposable
    {
        StreamWriter _writer;

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
        /// <param name="lineDelimiter">Разделитель строк (необязательный). В текущей релизации параметр игнорируется</param>
        /// <param name="append">Признак добавления в конец файла. (необязательный)</param>
        [ContextMethod("Открыть", "Open")]
        public void Open(string path, IValue encoding = null, string lineDelimiter = null, bool append = false)
        {
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
            
            _writer.Write(what);
        }

        /// <summary>
        /// Записывает текст и добавляет перевод строки
        /// </summary>
        /// <param name="what">Текст для записи</param>
        [ContextMethod("ЗаписатьСтроку", "WriteLine")]
        public void WriteLine(string what)
        {
            ThrowIfNotOpened();

            _writer.WriteLine(what);
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

        [ScriptConstructor(Name = "")]
        public static IRuntimeContextInstance Constructor(IValue path, IValue encoding)
        {
            return new TextWriteImpl(path.AsString(), encoding);
        }

        /// <summary>
        /// Создает объект с начальными значениями имени файла и кодировки.
        /// </summary>
        /// <param name="path">Имя файла</param>
        /// <param name="encoding">Кодировка в виде строки</param>
        /// <param name="lineDelimiter">Символ - разделитель строк</param>
        /// <param name="append">Признак добавления в конец файла (необязательный)</param>
        [ScriptConstructor(Name = "По имени файла и кодировке")]
        public static IRuntimeContextInstance Constructor(IValue path, IValue encoding = null, IValue lineDelimiter = null, IValue append = null)
        {
            bool isAppend = append != null && append.AsBoolean();
            return new TextWriteImpl(path.AsString(), encoding, isAppend);
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new TextWriteImpl();
        }

    }
}
