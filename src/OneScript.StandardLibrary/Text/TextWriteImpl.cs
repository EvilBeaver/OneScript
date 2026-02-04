/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Types;
using OneScript.Values;
using OneScript.StandardLibrary.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Text
{
    [ContextClass("ЗаписьТекста", "TextWriter")]
    public class TextWriteImpl : AutoContext<TextWriteImpl>, IDisposable
    {
        StreamWriter _writer;
        IStreamWrapper _streamWrapper;
        Encoding _encoding;
        string _lineDelimiter = "";
        string _eolReplacement = "";

        public TextWriteImpl()
        {

        }
        
        public TextWriteImpl(string path, IValue encoding)
        {
            Open(ValueFactory.Create(path), encoding);
        }

        public TextWriteImpl(string path, IValue encoding, bool append)
        {
            Open(ValueFactory.Create(path), encoding, null, ValueFactory.Create(append));
        }

        public TextWriteImpl(IValue stream, IValue encoding, IValue writeBom)
        {
            Open(stream, encoding, null, null, writeBom);
        }
        
        /// <summary>
        /// Открывает файл или устанавливает поток для записи.
        /// </summary>
        /// <param name="fileOrStream">Имя файла или поток, в который будет выполнена запись.</param>
        /// <param name="encoding">Кодировка (необязательный). По умолчанию используется utf-8</param>
        /// <param name="lineDelimiter">
        /// Определяет строку, разделяющую строки в файле/потоке (необязательный).
        /// Значение по умолчанию: ПС.</param>
        /// <param name="param4">
        /// Для файла:
        ///  Признак добавления в конец файла (необязательный).
        ///  Значение по умолчанию: Ложь.
        /// Для потока:
        ///  Определяет разделение строк в потоке для конвертации в стандартный перевод строк ПС (необязательный).
        ///  Значение по умолчанию: ВК + ПС.</param>
        /// <param name="param5">
        /// Для файла:
        ///  Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС (необязательный).
        ///  Значение по умолчанию: ВК + ПС.
        /// Для потока:
        ///  Если в начало потока требуется записать метку порядка байтов (BOM) для используемой кодировки текста,
        ///  то данный параметр должен иметь значение Истина.
        ///  Значение по умолчанию: Ложь.</param>
        [ContextMethod("Открыть", "Open")]
        public void Open(IValue fileOrStream, IValue encoding = null, string lineDelimiter = null, IValue param4 = null, IValue param5 = null)
        {
            if (fileOrStream is IStreamWrapper streamWrapper)
            {
                OpenStream(
                    streamWrapper,
                    encoding,
                    lineDelimiter,
                    ContextValuesMarshaller.ConvertValueStrict<string>(param4),
                    ContextValuesMarshaller.ConvertValueStrict<bool>(param5));
            }
            else
            {
                OpenFile(
                    fileOrStream.ToString(),
                    encoding,
                    lineDelimiter,
                    ContextValuesMarshaller.ConvertValueStrict<bool>(param4),
                    ContextValuesMarshaller.ConvertValueStrict<string>(param5));
            }
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
            PrepareForWriting();
            
            var stringToOutput = what.Replace ("\n", _eolReplacement);
            
            _writer.Write(stringToOutput);
        }

        /// <summary>
        /// Записывает текст и добавляет перевод строки
        /// </summary>
        /// <param name="what">Текст для записи</param>
        /// <param name="delimiter">Разделитель строк</param>
        [ContextMethod("ЗаписатьСтроку", "WriteLine")]
        public void WriteLine(IBslProcess process, string what, BslValue delimiter = null)
        {
            Write(what);

            var sDelimiter = _lineDelimiter;
            if (delimiter != null && delimiter.SystemType != BasicTypes.Undefined)
                sDelimiter = delimiter.ToString(process);

            Write(sDelimiter);
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

            _streamWrapper = null;
        }
        
        private void PrepareForWriting()
        {
            ThrowIfClosedStream();
            ThrowIfNonWritableStream();
            
            if (_writer == null && _streamWrapper != null)
            {
                _writer = new StreamWriter(_streamWrapper.GetUnderlyingStream(), _encoding, -1, true);
                _writer.AutoFlush = true;
            }
            
            ThrowIfNotOpened();
        }

        private void OpenFile(string path, IValue encoding = null, string lineDelimiter = null, bool append = false, string eolReplacement = null)
        {
            Dispose();
            
            _lineDelimiter = lineDelimiter ?? "\n";
            _eolReplacement = eolReplacement ?? "\r\n";
            _encoding = ResolveEncodingForFile(encoding, append);
            _streamWrapper = null;

            _writer = new StreamWriter(path, append, _encoding);
            _writer.AutoFlush = true;
        }

        private void OpenStream(IStreamWrapper streamWrapper, IValue encoding = null, string lineDelimiter = null, string eolReplacement = null, bool writeBom = false)
        {
            Dispose();
            
            _lineDelimiter = lineDelimiter ?? "\n";
            _eolReplacement = eolReplacement ?? "\r\n";
            _encoding = ResolveEncodingForStream(encoding, writeBom);
            _streamWrapper = streamWrapper;
        }
        
        private Encoding ResolveEncodingForFile(IValue encoding, bool append)
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
            return enc;
        }
        
        private Encoding ResolveEncodingForStream(IValue encoding, bool writeBom)
        {
            if (encoding == null)
            {
                return new UTF8Encoding(writeBom);
            }
            else
            {
                return TextEncodingEnum.GetEncoding(encoding, writeBom);
            }
        }
        
        private void ThrowIfClosedStream()
        {
            if (_streamWrapper != null)
            {
                var stream = _streamWrapper.GetUnderlyingStream();
                if (stream is { CanWrite: false, CanRead: false })
                    throw RuntimeException.ClosedStream();              
            }
        }           

        private void ThrowIfNonWritableStream()
        {
            if (_streamWrapper != null && _streamWrapper.IsReadOnly)
                throw RuntimeException.NonWritableStream();
        }  
        
        /// <summary>
        /// Создает объект для записи текста в файл или поток.
        /// </summary>
        /// <param name="fileOrStream">Имя файла или поток, в который будет выполнена запись.</param>
        /// <param name="encoding">Кодировка (необязательный). По умолчанию используется utf-8</param>
        /// <param name="lineDelimiter">
        /// Определяет строку, разделяющую строки в файле/потоке (необязательный).
        /// Значение по умолчанию: ПС.</param>
        /// <param name="param4">
        /// Для файла:
        ///  Признак добавления в конец файла (необязательный).
        ///  Значение по умолчанию: Ложь.
        /// Для потока:
        ///  Определяет разделение строк в потоке для конвертации в стандартный перевод строк ПС (необязательный).
        ///  Значение по умолчанию: ВК + ПС.</param>
        /// <param name="param5">
        /// Для файла:
        ///  Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС (необязательный).
        ///  Значение по умолчанию: ВК + ПС.
        /// Для потока:
        ///  Если в начало потока требуется записать метку порядка байтов (BOM) для используемой кодировки текста,
        ///  то данный параметр должен иметь значение Истина.
        ///  Значение по умолчанию: Ложь.</param>
        [ScriptConstructor(Name = "По имени файла")]
        public static TextWriteImpl Constructor(IValue fileOrStream, IValue encoding = null, string lineDelimiter = null, IValue param4 = null, IValue param5 = null)
        {
            var result = new TextWriteImpl();
            result.Open(fileOrStream, encoding, lineDelimiter, param4, param5);
            return result;
        }
        
        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static TextWriteImpl Constructor()
        {
            return new TextWriteImpl();
        }

    }
}