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
using OneScript.StandardLibrary.Text;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Json
{
    /// <summary>
    /// 
    /// Предназначен для организации последовательной записи объектов и текстов JSON.
    /// </summary>
    [ContextClass("ЗаписьJSON", "JSONWriter")]
    public class JSONWriter : AutoContext<JSONWriter>
    {
        private const int INDENT_SIZE = 0;

        private JSONWriterSettings _settings;
        private JsonTextWriter _writer; // Объект из библиотеки Newtonsoft для работы с форматом JSON 

        StringWriter _stringWriter;

        public JSONWriter()
        {
            
        }

        private bool _escapeNonAscii;

        /// <summary>
        /// 
        /// Возвращает true если для объекта чтения json был задан текст для парсинга.
        /// </summary>
        private bool IsOpen()
        {
            return _writer != null;
        }

        /// <summary>
        /// 
        /// Возвращает true если для объекта чтения json был задан текст для парсинга.
        /// </summary>
        private bool IsOpenForString()
        {
            return _stringWriter != null;
        }

        private void SetDefaultOptions()
        {
            _writer.Indentation = INDENT_SIZE;
            _writer.Formatting = Formatting.Indented;
            _settings = new JSONWriterSettings();
            _escapeNonAscii = false;
        }

        private void SetOptions(IValue settings)
        {
            _settings = (JSONWriterSettings)settings.GetRawValue();
            if (_settings.UseDoubleQuotes)
                _writer.QuoteChar = '\"';
            else { 
                _writer.QuoteChar = '\'';
            }

            if (_settings.PaddingSymbols != null && _settings.PaddingSymbols.Length > 0)
                _writer.IndentChar = _settings.PaddingSymbols[0];
            else
                _writer.IndentChar = ' ';

            if (_settings.PaddingSymbols != null && _settings.PaddingSymbols.Length > 0)
            {
                _writer.Indentation = 1;
            }
            else
            {
                _writer.Indentation = INDENT_SIZE;
            }
            _writer.Formatting = Formatting.Indented;

            if (_settings.EscapeCharacters != null)
            {
                var jsonCharactersEscapeMode = _settings.EscapeCharacters.GetRawValue() as SelfAwareEnumValue<JSONCharactersEscapeModeEnum>;
                var jsonCharactersEscapeModeEnum = GlobalsHelper.GetEnum<JSONCharactersEscapeModeEnum>();

                if (jsonCharactersEscapeMode == jsonCharactersEscapeModeEnum.NotASCIISymbols)
                {
                    _escapeNonAscii = true;
                    _writer.QuoteChar = '\"';
                    _writer.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
                }
                else if (jsonCharactersEscapeMode == jsonCharactersEscapeModeEnum.SymbolsNotInBMP)
                    throw new NotImplementedException();
            }
        }

        void WriteStringValue(string val)
        { 
            if (_settings.EscapeCharacters != null && _escapeNonAscii)
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
            var sb = new StringBuilder(sval);

            int Length = sval.Length;
            for (var i = 0; i < Length; i++)
            {
                char c = sb[i];
                if (EscapeSlash && c == '/')
                {
                    sb.Replace("/", "\\/", i, 1);
                    Length++;
                    i++;
                }
                else if (_settings.EscapeAmpersand && c == '&')
                {
                    sb.Replace("&", "\\&", i, 1);
                    Length++;
                    i++;
                }
                else if ((_settings.EscapeSingleQuotes || !_settings.UseDoubleQuotes) && c == '\'')
                {
                    sb.Replace("'", "\\u0027", i, 1);
                    Length = Length + 5;
                    i = i + 5;
                }
                else if (_settings.EscapeAngleBrackets && c == '<')
                {
                    sb.Replace("<", "\\u003C", i, 1);
                    Length = Length + 5;
                    i = i + 5;
                }
                else if (_settings.EscapeAngleBrackets && c == '>')
                {
                    sb.Replace(">", "\\u003E", i, 1);
                    Length = Length + 5;
                    i = i + 5;
                }
                else if (c == '\r')
                {
                    sb.Replace("\r", "\\r", i, 1);
                    Length++;
                    i++;
                }
                else if (c == '\n')
                {
                    sb.Replace("\n", "\\n", i, 1);
                    Length++;
                    i++;
                }
                else if (c == '\f')
                {
                    sb.Replace("\f", "\\f", i, 1);
                    Length++;
                    i++;
                }
                else if (c == '\"')
                {
                    sb.Replace("\"", "\\\"", i, 1);
                    Length++;
                    i++;
                }
                else if (c == '\b')
                {
                    sb.Replace("\b", "\\b", i, 1);
                    Length++;
                    i++;
                }
                else if (c == '\t')
                {
                    sb.Replace("\t", "\\t", i, 1);
                    Length++;
                    i++;
                }
                else if (c == '\\')
                {
                    sb.Replace("\\", "\\\\", i, 1);
                    Length++;
                    i++;
                }

                // Спец. символы: \u0000, \u0001, \u0002, ... , \u001e, \u001f;
                else if ((int)c >= 0 && (int)c <= 31)
                {
                    string unicode = "\\u" + ((int)c).ToString("X4").ToLower();
                    sb.Replace(c.ToString(), unicode, i, 1);
                    Length = Length + 5;
                    i = i + 5;
                }

            }
            sb.Insert(0, _writer.QuoteChar);
            sb.Append(_writer.QuoteChar);
            return sb.ToString();
        }

        RuntimeException NotOpenException()
        {
            return new RuntimeException(Locale.NStr("ru='Приемник данных JSON не открыт';en='JSON data target is not opened'"));
        }

        void SetNewLineChars(TextWriter textWriter)
        {
            if (_settings != null)
            {
                if (_settings.NewLines != null)
                {
                    var NewLines = _settings.NewLines.GetRawValue() as SelfAwareEnumValue<JSONLineBreakEnum>;
                    var LineBreakEnum = GlobalsHelper.GetEnum<JSONLineBreakEnum>();

                    if (NewLines == LineBreakEnum.Unix)
                        textWriter.NewLine = "\n";
                    else if (NewLines == LineBreakEnum.Windows)
                        textWriter.NewLine = "\r\n";
                    else if (NewLines == LineBreakEnum.Auto)
                    {
                        if (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX)
                            textWriter.NewLine = "\n";
                        else
                            textWriter.NewLine = "\r\n";
                    }
                    else
                    {
                        textWriter.NewLine = ""; //Нет
                        _writer.Formatting = Formatting.None;
                    }
                        
                }

            }
        }
        [ScriptConstructor]
        public static JSONWriter Constructor()
        {
            return new JSONWriter();
        }

        /// <summary>
        /// 
        /// Определяет текущие параметры записи JSON.
        /// </summary>
        /// <value>ПараметрыЗаписиJSON (JSONWriterSettings)</value>
        [ContextProperty("Параметры", "Settings")]
        public IValue Settings
        {
            get { throw new NotImplementedException(); }

        }


        /// <summary>
        /// 
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
        /// 
        /// Завершает запись текста JSON. Если производилась запись в файл, то файл закрывается. Если производилась запись в строку, то результирующая строка будет получена в качестве возвращаемого значения метода. Если производилась запись в файл, то метод вернет пустую строку.
        /// </summary>
        ///
        /// <returns name="String">
        /// Значения данного типа содержат строку в формате Unicode произвольной длины.</returns>
        [ContextMethod("Закрыть", "Close")]
        public string Close()
        {
            string res = "";

            if (IsOpenForString())
            {
                res = _stringWriter.ToString();
			}

            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }

            return res;
        }


        /// <summary>
        /// 
        /// Выполняет запись произвольной строки в документ, при этом проверка структуры документа не выполняется.
        /// Если при использовании метода свойство ПроверятьСтруктуру установлено в значение Истина, то проверка структуры продолжается на следующем элементе.
        /// </summary>
        ///
        /// <param name="stringValue">
        /// Строка, записываемая в документ JSON. </param>
        [ContextMethod("ЗаписатьБезОбработки", "WriteRaw")]
        public void WriteRaw(string stringValue)
        {
            if (!IsOpen())
                throw NotOpenException();

            _writer.WriteRaw(stringValue);
        }


        /// <summary>
        /// 
        /// Записывает значение свойства JSON.
        /// </summary>
        ///
        /// <param name="value">
        /// Записываемое значение. Типы: Строка (String), Число (Number), Булево (Boolean), Неопределено (Undefined) </param>
        /// <param name="useFormatWithExponent">
        /// Использование экспоненциальной формы записи для числовых значений. Параметр имеет смысл только если записывается значение числового типа.
        /// Значение по умолчанию: Ложь. </param>
        [ContextMethod("ЗаписатьЗначение", "WriteValue")]
        public void WriteValue(IValue value, bool useFormatWithExponent = false)
        {
            if (!IsOpen())
                throw NotOpenException();

            if(!(value.GetRawValue() is BslPrimitiveValue))
                throw new RuntimeException("Тип переданного значения не поддерживается.");
            
            var clrValue = value.CastToClrObject();
            
            if (clrValue == null)
            {
                _writer.WriteNull();
                return;
            }

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
                    WriteStringValue(v.ToString());
                    break;
            }
        }


        /// <summary>
        /// 
        /// Записывает имя свойства JSON.
        /// </summary>
        ///
        /// <param name="propertyName">
        /// Имя свойства. </param>
        [ContextMethod("ЗаписатьИмяСвойства", "WritePropertyName")]
        public void WritePropertyName(string propertyName)
        {
            _writer.WritePropertyName(propertyName);
        }


        /// <summary>
        /// 
        /// Записывает конец массива JSON.
        /// </summary>
        ///
        [ContextMethod("ЗаписатьКонецМассива", "WriteEndArray")]
        public void WriteEndArray()
        {
            _writer.WriteEndArray();
        }


        /// <summary>
        /// 
        /// Записывает конец объекта JSON.
        /// </summary>
        ///
        [ContextMethod("ЗаписатьКонецОбъекта", "WriteEndObject")]
        public void WriteEndObject()
        {
            _writer.WriteEndObject();
        }


        /// <summary>
        /// 
        /// Записывает начало массива JSON.
        /// </summary>
        ///
        [ContextMethod("ЗаписатьНачалоМассива", "WriteStartArray")]
        public void WriteStartArray()
        {
            _writer.WriteStartArray();
        }


        /// <summary>
        /// 
        /// Записывает начало объекта JSON.
        /// </summary>
        ///
        [ContextMethod("ЗаписатьНачалоОбъекта", "WriteStartObject")]
        public void WriteStartObject()
        {
            _writer.WriteStartObject();
        }


        /// <summary>
        /// 
        /// Открывает файл для записи JSON. Позволяет указать тип кодировки, который будет использован для записи файла JSON, а также использование BOM.
        /// </summary>
        ///
        /// <param name="fileName">
        /// Имя файла, в который будет записываться текст JSON. </param>
        /// <param name="encoding">
        /// В качестве типа кодировки может быть указана одна из возможных кодировок текста. В этом случае файл будет записан в соответствующей кодировке. Если же в качестве параметра указана пустая строка или ничего не указано, то для записи файла будет использована кодировка UTF8.
        /// Поддерживаемые коды кодировок:
        /// 
        /// Значение по умолчанию: UTF-8. </param>
        /// <param name="addBOM">
        /// Определяет, будет ли добавлен маркер порядка байт (BOM) к результирующему файлу JSON.
        /// Внимание. Стандарт RFC7159 настоятельно рекомендует не добавлять маркер порядка байт (BOM) к документу JSON .
        /// Значение по умолчанию: Ложь. </param>
        /// <param name="settings">
        /// Параметры, используемые при открытии файла для настройки записи в формате JSON. </param>
        [ContextMethod("ОткрытьФайл", "OpenFile")]
        public void OpenFile(string fileName, string encoding = null, IValue addBOM = null, IValue settings = null)
        {
            if (IsOpen())
                Close();

            bool bAddBOM = false;
            if (addBOM != null)
                bAddBOM = addBOM.AsBoolean();

            StreamWriter streamWriter;
            
            try
            {
                if (encoding != null)
                    streamWriter = ScriptEngine.Environment.FileOpener.OpenWriter(fileName, TextEncodingEnum.GetEncodingByName(encoding, bAddBOM));
                else
                    streamWriter = ScriptEngine.Environment.FileOpener.OpenWriter(fileName, TextEncodingEnum.GetEncodingByName("UTF-8", bAddBOM));
            }
            catch (Exception e)
            {
                throw new RuntimeException(e.Message, e);
            }


            _writer = new JsonTextWriter(streamWriter);
            if (settings == null)
                SetDefaultOptions();
            else
                SetOptions(settings);

            SetNewLineChars(streamWriter);
        }


        /// <summary>
        /// 
        /// Инициализирует объект для вывода результирующего JSON текста в строку.
        /// </summary>
        ///
        /// <param name="settings">
        /// Параметры, используемые при записи объекта JSON.
        /// По умолчанию, содержит ПараметрыЗаписиJSON, сгенерированные автоматически. </param>
        [ContextMethod("УстановитьСтроку", "SetString")]
        public void SetString(IValue settings = null)
        {
            if (IsOpen())
                Close();
            _stringWriter = new StringWriter();
            _writer = new JsonTextWriter(_stringWriter);
            if (settings == null)
                SetDefaultOptions();
            else
                SetOptions(settings);

            SetNewLineChars(_stringWriter);
        }

    }
}
