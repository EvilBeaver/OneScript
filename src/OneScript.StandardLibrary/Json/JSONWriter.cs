/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Json
{
    /// <summary>
    /// Предназначен для организации последовательной записи объектов и текстов JSON.
    /// </summary>
    [ContextClass("ЗаписьJSON", "JSONWriter")]
    public class JSONWriter : AutoContext<JSONWriter>, IDisposable
    {
        private const int INDENT_SIZE = 0;
        private const string DEFAULT_ENCODING = "UTF-8";

        private JSONWriterSettings _settings;
        private JsonTextWriter _writer; // Объект из библиотеки Newtonsoft для работы с форматом JSON 

        StringWriter _stringWriter;
        private bool _escapeNonAscii;

        public JSONWriter()
        {
            
        }

        /// <summary>
        /// Возвращает true если был открыт объект для записи.
        /// </summary>
        private bool IsOpen()
        {
            return _writer != null;
        }

        /// <summary>
        /// Возвращает true если текст json выводится в строку.
        /// </summary>
        private bool IsOpenForString()
        {
            return _stringWriter != null;
        }

        private void CheckWriter()
        {
            if (!IsOpen())
                throw NotOpenException();
        }

        private void SetDefaultOptions()
        {
            _writer.Indentation = INDENT_SIZE;
            _writer.Formatting = Formatting.Indented;
            _settings = new JSONWriterSettings();
            _escapeNonAscii = false;
        }

        private void SetOptions(JSONWriterSettings settings)
        {
            if (settings == null)
            {
                SetDefaultOptions();
                return;
            }

            _settings = settings;
            if (_settings.UseDoubleQuotes)
                _writer.QuoteChar = '\"';
            else { 
                _writer.QuoteChar = '\'';
            }

            _writer.IndentChar = !string.IsNullOrEmpty(_settings.PaddingSymbols) ? _settings.PaddingSymbols[0] : ' ';
            _writer.Indentation = !string.IsNullOrEmpty(_settings.PaddingSymbols) ? 1 : INDENT_SIZE;
            _writer.Formatting = Formatting.Indented;

            if (_settings.EscapeCharacters != JSONCharactersEscapeModeEnum.None)
            {
                var jsonCharactersEscapeMode = _settings.EscapeCharacters;
                if (jsonCharactersEscapeMode == JSONCharactersEscapeModeEnum.NotASCIISymbols)
                {
                    _escapeNonAscii = true;
                    _writer.QuoteChar = '\"';
                    _writer.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
                }
                else if (jsonCharactersEscapeMode == JSONCharactersEscapeModeEnum.SymbolsNotInBMP)
                    throw new NotImplementedException();
            }
        }

        void WriteStringValue(string val)
        { 
            if (_settings.EscapeCharacters != JSONCharactersEscapeModeEnum.None && _escapeNonAscii)
            {
                StringWriter wr = new StringWriter();
                var jsonWriter = new JsonTextWriter(wr);
                jsonWriter.QuoteChar = '\"';
                jsonWriter.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
                new JsonSerializer().Serialize(jsonWriter, val);
                string str = wr.ToString();
                _writer.WriteRawValue(EscapeCharacters(str.Substring(1, str.Length - 2), false));

            }
            else
                _writer.WriteRawValue(EscapeCharacters(val, _settings.EscapeSlash));
        }

        string EscapeCharacters(string sval, bool EscapeSlash)
        {
            var sb = new StringBuilder();

            int length = sval.Length;
            int start = 0;

            for (var i = 0; i < length; i++)
            {
                char c = sval[i];
                string? escapedValue = null;

                if (EscapeSlash && c == '/')
                {
                    escapedValue = "\\/";
                }
                else if (_settings.EscapeAmpersand && c == '&')
                {
                    escapedValue = "\\&";
                }
                else if ((_settings.EscapeSingleQuotes || !_settings.UseDoubleQuotes) && c == '\'')
                {
                    escapedValue = "\\u0027";
                }
                else if (_settings.EscapeAngleBrackets && c == '<')
                {
                    escapedValue = "\\u003C";
                }
                else if (_settings.EscapeAngleBrackets && c == '>')
                {
                    escapedValue = "\\u003E";
                }
                else if (c == '\r')
                {
                    escapedValue = "\\r";
                }
                else if (c == '\n')
                {
                    escapedValue = "\\n";
                }
                else if (c == '\f')
                {
                    escapedValue = "\\f";
                }
                else if (c == '\"')
                {
                    escapedValue = "\\\"";
                }
                else if (c == '\b')
                {
                    escapedValue = "\\b";
                }
                else if (c == '\t')
                {
                    escapedValue = "\\t";
                }
                else if (c == '\\')
                {
                    escapedValue = "\\\\";
                }

                // Спец. символы: \u0000, \u0001, \u0002, ... , \u001e, \u001f;
                else if ((int)c >= 0 && (int)c <= 31)
                {
                    escapedValue = "\\u" + ((int)c).ToString("x4");
                }

                if (escapedValue != null)
                {
                    sb.Append(sval, start, i - start);
                    sb.Append(escapedValue);
                    start = i + 1;
                }
            }

            sb.Insert(0, _writer.QuoteChar);
            sb.Append(sval, start, length - start);
            sb.Append(_writer.QuoteChar);
            return sb.ToString();
        }

        void SetNewLineChars(TextWriter textWriter)
        {
            if (_settings != null)
            {
                switch (_settings.NewLines)
                {
                    case JSONLineBreakEnum.Unix:
                        textWriter.NewLine = "\n";
                        break;
                    case JSONLineBreakEnum.Windows:
                        textWriter.NewLine = "\r\n";
                        break;
                    case JSONLineBreakEnum.Auto when Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX:
                        textWriter.NewLine = "\n";
                        break;
                    case JSONLineBreakEnum.Auto:
                        textWriter.NewLine = "\r\n";
                        break;
                    default:
                        textWriter.NewLine = ""; //Нет
                        _writer.Formatting = Formatting.None;
                        break;
                }
            }
        }
        [ScriptConstructor]
        public static JSONWriter Constructor()
        {
            return new JSONWriter();
        }

        /// <summary>
        /// Определяет текущие параметры записи JSON.
        /// </summary>
        /// <value>ПараметрыЗаписиJSON (JSONWriterSettings)</value>
        [ContextProperty("Параметры", "Settings")]
        public IValue Settings
        {
            get { throw new NotImplementedException(); }

        }

        /// <summary>
        /// Показывает, будет ли проводиться проверка правильности структуры записываемого JSON объекта. В случае обнаружение ошибки, будет сгенерировано исключение. Например: при попытке записать значение без имени вне массива или записать окончание объекта без начала. Установка данного свойства не имеет немедленного эффекта. Установленное значение свойства будет использовано только после открытия файла или установки строки.
        /// После создания объекта данное свойство имеет значение Истина.
        /// </summary>
        /// <value>Булево (Boolean)</value>
        [ContextProperty("ПроверятьСтруктуру", "ValidateStructure")]
        public bool ValidateStructure
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }


        /// <summary>
        /// Завершает запись текста JSON.
        /// Если производилась запись в строку, то метод вернет результирующую строку.
        /// Если производилась запись в файл, то файл закрывается, а метод вернет пустую строку.
        /// </summary>
        /// <remarks>Допускается повторное закрытие. Будет возвращена пустая строка.</remarks>
        /// <returns name="String">Строка в формате Unicode</returns>
        [ContextMethod("Закрыть", "Close")]
        public string Close()
        {
            var result = String.Empty;
            if (_writer is not null)
            {
                _writer.Flush();
                if (IsOpenForString())
                    result = _stringWriter.ToString();

                Dispose();
            }

            return result;
        }

        /// <summary>
        /// Выполняет запись произвольной строки в документ, при этом проверка структуры документа не выполняется.
        /// Если при использовании метода свойство ПроверятьСтруктуру установлено в значение Истина, то проверка структуры продолжается на следующем элементе.
        /// </summary>
        /// <param name="stringValue">
        /// Строка, записываемая в документ JSON. </param>
        [ContextMethod("ЗаписатьБезОбработки", "WriteRaw")]
        public void WriteRaw(string stringValue)
        {
            CheckWriter();
            _writer.WriteRaw(stringValue);
        }


        /// <summary>
        /// Записывает значение свойства JSON.
        /// </summary>
        /// <param name="value">
        /// Записываемое значение. Типы: Строка (String), Число (Number), Булево (Boolean), Неопределено (Undefined) </param>
        /// <param name="useFormatWithExponent">
        /// Использование экспоненциальной формы записи для числовых значений. Параметр имеет смысл только если записывается значение числового типа.
        /// Значение по умолчанию: Ложь. </param>
        [ContextMethod("ЗаписатьЗначение", "WriteValue")]
        public void WriteValue(IValue value, bool useFormatWithExponent = false)
        {
            CheckWriter();

            var clrValue = value.UnwrapToClrObject();
            switch (clrValue)
            {
                case string v:
                     WriteStringValue(v);
                    break;
                case decimal v:
                    if (v == Math.Round(v))
                    {
                        Int64 i  = Convert.ToInt64(v);
                        if (useFormatWithExponent)
                            _writer.WriteRawValue(string.Format(Thread.CurrentThread.CurrentCulture, "{0:E}", i));
                        else
                            _writer.WriteValue(i);
                    }
                    else
                    {
                        if (useFormatWithExponent)
                            _writer.WriteRawValue(string.Format(string.Format(Thread.CurrentThread.CurrentCulture, "{0:E}", v)));
                        else
                            _writer.WriteValue(v);
                    }
                    break;
                case bool v:
                    _writer.WriteValue(v);
                    break;
                case DateTime v:
                    _writer.WriteValue(v);
                    break;

                case null:
                    _writer.WriteNull();
                    break;

                default:
                    throw TypeNotSupportedException(value?.GetType());
            }
        }

        /// <summary>
        /// Записывает имя свойства JSON.
        /// </summary>
        /// <param name="propertyName">
        /// Имя свойства. </param>
        [ContextMethod("ЗаписатьИмяСвойства", "WritePropertyName")]
        public void WritePropertyName(string propertyName)
        {
            CheckWriter();
            _writer.WritePropertyName(propertyName);
        }


        /// <summary>
        /// Записывает конец массива JSON.
        /// </summary>
        [ContextMethod("ЗаписатьКонецМассива", "WriteEndArray")]
        public void WriteEndArray()
        {
            CheckWriter();
            _writer.WriteEndArray();
        }


        /// <summary>
        /// Записывает конец объекта JSON.
        /// </summary>
        [ContextMethod("ЗаписатьКонецОбъекта", "WriteEndObject")]
        public void WriteEndObject()
        {
            CheckWriter();
            _writer.WriteEndObject();
        }


        /// <summary>
        /// Записывает начало массива JSON.
        /// </summary>
        [ContextMethod("ЗаписатьНачалоМассива", "WriteStartArray")]
        public void WriteStartArray()
        {
            CheckWriter();
            _writer.WriteStartArray();
        }


        /// <summary>
        /// Записывает начало объекта JSON.
        /// </summary>
        [ContextMethod("ЗаписатьНачалоОбъекта", "WriteStartObject")]
        public void WriteStartObject()
        {
            CheckWriter();
            _writer.WriteStartObject();
        }


        /// <summary>
        /// Открывает файл для записи JSON. Позволяет указать тип кодировки, который будет использован для записи файла JSON, а также использование BOM.
        /// </summary>
        /// <param name="fileName">
        /// Имя файла, в который будет записываться текст JSON. </param>
        /// <param name="encoding">
        /// В качестве типа кодировки может быть указана одна из возможных кодировок текста. В этом случае файл будет записан в соответствующей кодировке. Если же в качестве параметра указана пустая строка или ничего не указано, то для записи файла будет использована кодировка UTF8.
        /// Поддерживаемые коды кодировок:
        /// Значение по умолчанию: UTF-8. </param>
        /// <param name="addBOM">
        /// Определяет, будет ли добавлен маркер порядка байт (BOM) к результирующему файлу JSON.
        /// Внимание. Стандарт RFC7159 настоятельно рекомендует не добавлять маркер порядка байт (BOM) к документу JSON .
        /// Значение по умолчанию: Ложь. </param>
        /// <param name="settings">
        /// Параметры, используемые при открытии файла для настройки записи в формате JSON. </param>
        [ContextMethod("ОткрытьФайл", "OpenFile")]
        public void OpenFile(string fileName, string encoding = null, bool addBOM = false, JSONWriterSettings settings = null)
        {
            StreamWriter streamWriter;
            var textEncoding = TextEncodingEnum.GetEncodingByName(encoding ?? DEFAULT_ENCODING, addBOM);

            try
            {
               streamWriter = FileOpener.OpenWriter(fileName, textEncoding);
            }
            catch (Exception e)
            {
                throw new RuntimeException(e.Message, e);
            }

            Close();
            SetWriter(streamWriter, settings);
        }

        /// <summary>
        /// Открывает поток для записи JSON. Позволяет указать тип кодировки, который будет использован для записи файла JSON, а также использование BOM.
        /// </summary>
        /// <param name="streamContext">
        /// Поток, в который будет записываться текст JSON. </param>
        /// <param name="encoding">
        /// В качестве типа кодировки может быть указана одна из возможных кодировок текста. В этом случае файл будет записан в соответствующей кодировке. Если же в качестве параметра указана пустая строка или ничего не указано, то для записи файла будет использована кодировка UTF8.
        /// Поддерживаемые коды кодировок:
        /// Значение по умолчанию: UTF-8. </param>
        /// <param name="addBOM">
        /// Определяет, будет ли добавлен маркер порядка байт (BOM) к результирующему потоку JSON.
        /// Внимание. Стандарт RFC7159 настоятельно рекомендует не добавлять маркер порядка байт (BOM) к документу JSON .
        /// Значение по умолчанию: Ложь. </param>
        /// <param name="settings">
        /// Параметры, используемые при открытии потока для настройки записи в формате JSON.</param>
        [ContextMethod("ОткрытьПоток", "OpenStream")]
        public void OpenStream(IStreamWrapper streamContext, string encoding = null, bool addBOM = false, JSONWriterSettings settings = null)
        {
            if (streamContext is null)
                throw new ArgumentNullException(nameof(streamContext));

            if (streamContext.IsReadOnly)
                throw CannotWriteException();

            Close();
            var textEncoding = TextEncodingEnum.GetEncodingByName(encoding ?? DEFAULT_ENCODING, addBOM);
            StreamWriter streamWriter = new(streamContext.GetUnderlyingStream(), textEncoding);
            SetWriter(streamWriter, settings);
        }


        /// <summary>
        /// Инициализирует объект для вывода результирующего JSON текста в строку.
        /// </summary>
        /// <param name="settings">
        /// Параметры, используемые при записи объекта JSON.
        /// По умолчанию, содержит ПараметрыЗаписиJSON, сгенерированные автоматически. </param>
        [ContextMethod("УстановитьСтроку", "SetString")]
        public void SetString(JSONWriterSettings settings = null)
        {
            Close();

            _stringWriter = new StringWriter();
            SetWriter(_stringWriter, settings);
        }

        private void SetWriter(TextWriter stream, JSONWriterSettings settings)
        {
            _writer = new JsonTextWriter(stream);

            SetOptions(settings);
            SetNewLineChars(stream);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _writer is not null)
            {
                _stringWriter?.Dispose();
                _stringWriter = null;

                _writer.Close();
                _writer = null;
            }
        }

        #endregion

        RuntimeException NotOpenException()
        {
            return new RuntimeException(Locale.NStr
                ("ru='Приемник данных JSON не открыт';en='JSON data target is not opened'"));
        }

        RuntimeException CannotWriteException()
        {
            return new("Попытка записи в поток не поддерживающий запись",
                "Cannot write to a stream that does not support writing");
        }

        RuntimeException TypeNotSupportedException(Type type)
        {
            return new RuntimeException(Locale.NStr
                ($"ru='Запись значения типа {type} не поддерживается.'; en='Can not write value of type {type}'"));
        }

    }
}
