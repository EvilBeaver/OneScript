/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using Newtonsoft.Json;
using OneScript.Commons;
using OneScript.StandardLibrary.Text;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Json
{
    /// <summary>
    /// 
    /// Предназначен для последовательного чтения JSON-данных из файла или строки.
    /// </summary>
    [ContextClass("ЧтениеJSON", "JSONReader")]
    public class JSONReader : AutoContext<JSONReader>
    {

        private JsonTextReader _reader; // Объект из библиотеки Newtonsoft для работы с форматом JSON 

        RuntimeException NotOpenException()
        {
            return new RuntimeException(Locale.NStr("ru='Источник данных JSON не открыт'; en='JSON data source is not opened'"));
        }

        /// <summary>
        /// 
        /// Возвращает true если для объекта чтения json был задан текст для парсинга.
        /// </summary>
        private bool IsOpen()
        {
            return _reader != null;
        }

        public JSONReader()
        {
        }

        [ScriptConstructor]
        public static JSONReader Constructor()
        {
            return new JSONReader();
        }

        /// <summary>
        /// 
        /// Указывает на позицию, находящуюся сразу после прочитанного значения.
        /// При ошибке чтение остается на позиции последнего успешно считанного символа.
        /// </summary>
        /// <value>Число (Number), Неопределено (Undefined)</value>
        [ContextProperty("ТекущаяПозиция", "CurrentPosition")]
        public IValue CurrentPosition
        {
            get
            {
                if (IsOpen())
                {
                    return ValueFactory.Create(_reader.LinePosition);
                }
                else
                    return ValueFactory.Create(); // Неопределено 
            }

        }


        /// <summary>
        /// 
        /// Указывает на позицию сразу после прочитанного значения.
        /// Например, перед чтением первого элемента - 0, после чтения первого элемента -1 .
        /// </summary>
        /// <value>Число (Number), Неопределено (Undefined)</value>
        [ContextProperty("ТекущаяСтрока", "CurrentLine")]
        public IValue CurrentLine
        {
            get
            {
                if (IsOpen())
                {
                    return ValueFactory.Create(_reader.LineNumber);
                }
                else
                    return ValueFactory.Create(); // Неопределено
            }

        }


        /// <summary>
        /// 
        /// Содержит текущее значение:
        /// 
        ///  - Число - если ТипТекущегоЗначения имеет значение Число;
        ///  - Строка - если ТипТекущегоЗначения имеет значение:
        /// 
        ///  - Комментарий,
        ///  - ИмяСвойства,
        ///  - Строка;
        ///  - Булево - если ТипТекущегоЗначения имеет значение Булево,
        ///  - Неопределено - если ТипТекущегоЗначения имеет значение Null.
        /// Исключение генерируется в случае, если ТипТекущегоЗначения имеет одно из следующих значений:
        /// 
        ///  - НачалоМассива,
        ///  - КонецМассива,
        ///  - НачалоОбъекта,
        ///  - КонецОбъекта,
        ///  - Ничего.
        /// </summary>
        /// <value>Число (Number), Строка (String), Булево (Boolean), Неопределено (Undefined)</value>
        [ContextProperty("ТекущееЗначение", "CurrentValue")]
        public IValue CurrentValue
        {
            get
            {
                if (IsOpen())
                {
                    var type = _reader.TokenType;

                    if (type == JsonToken.String || type == JsonToken.Comment || type == JsonToken.PropertyName)
                        return ValueFactory.Create((string)_reader.Value);
                    else if (type == JsonToken.Boolean)
                        return ValueFactory.Create((bool)_reader.Value);
                    else if (type == JsonToken.Integer || type == JsonToken.Float)
                    {
                        decimal d = Convert.ToDecimal(_reader.Value);
                        return ValueFactory.Create(d);
                    }
                    else if (type == JsonToken.Date)
                    {
                        return ValueFactory.Create((DateTime)_reader.Value);
                    }
                    else if (type == JsonToken.Null)
                    {
                        return ValueFactory.CreateNullValue();
                    }
                    else if (type == JsonToken.Undefined)
                    {
                        return ValueFactory.Create();
                    }
                    else
                        throw new RuntimeException(Locale.NStr("ru='Текущее значение JSON не может быть получено';en='Cannot get current JSON value'"));
                }
                else
                {
                    throw NotOpenException();
                }
                    
            }
        }

        /// <summary>
        /// 
        /// Тип текущего значения в документе JSON во внутреннем формате.
        /// null - если чтение еще не началось или достигнут конец файла.
        /// </summary>
        /// <value>CurrentJsonTokenType</value>
        public JsonToken CurrentJsonTokenType
        {
            get
            {
                if (IsOpen())
                {
                    return _reader.TokenType;
                }
                else
                    throw NotOpenException();
            }
        }


        /// <summary>
        /// 
        /// Тип текущего значения в документе JSON.
        /// Неопределено - если чтение еще не началось или достигнут конец файла.
        /// </summary>
        /// <value>ТипЗначенияJSON (JSONValueType)</value>
        [ContextProperty("ТипТекущегоЗначения", "CurrentValueType")]
        public IValue CurrentValueType
        {
            get
            {
                if (IsOpen())
                {
                    string JSONValueType = "None";

                    switch (_reader.TokenType)
                    {
                        case JsonToken.Null:
                            JSONValueType = "Null";
                            break;
                        case JsonToken.StartObject:
                            JSONValueType = "ObjectStart";
                            break;
                        case JsonToken.StartArray:
                            JSONValueType = "ArrayStart";
                            break;
                        case JsonToken.PropertyName:
                            JSONValueType = "PropertyName";
                            break;
                        case JsonToken.Comment:
                            JSONValueType = "Comment";
                            break;
                        case JsonToken.Integer:
                            JSONValueType = "Number";
                            break;
                        case JsonToken.Float:
                            JSONValueType = "Number";
                            break;
                        case JsonToken.String:
                            JSONValueType = "String";
                            break;
                        case JsonToken.Boolean:
                            JSONValueType = "Boolean";
                            break;
                        case JsonToken.Undefined:
                            JSONValueType = "Null";
                            break;
                        case JsonToken.EndObject:
                            JSONValueType = "ObjectEnd";
                            break;
                        case JsonToken.EndArray:
                            JSONValueType = "ArrayEnd";
                            break;
                    }
                    return GlobalsHelper.GetEnum<JSONValueTypeEnum>()[JSONValueType];
                }
                else
                    throw NotOpenException();
            }

        }

        /// <summary>
        /// 
        /// Завершает чтение текста JSON из файла или строки.
        /// </summary>
        ///
        ///
        ///
        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {

            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }


        /// <summary>
        /// 
        /// Открывает JSON-файл для чтения данным объектом. Если перед вызовом данного метода уже производилось чтение JSON из другого файла или строки, то чтение прекращается и объект инициализируется для чтения из указанного файла.
        /// </summary>
        ///
        /// <param name="JSONFileName">
        /// Имя файла, содержащего текст JSON. </param>
        /// <param name="encoding">
        /// Позволяет задать кодировку входного файла.</param>
        [ContextMethod("ОткрытьФайл", "OpenFile")]
        public void OpenFile(string JSONFileName, IValue encoding = null)
        {
   
            if (IsOpen())
                Close();

            StreamReader _fileReader;

            try
            {
                if (encoding != null)
                    _fileReader = ScriptEngine.Environment.FileOpener.OpenReader(JSONFileName, TextEncodingEnum.GetEncoding(encoding));
                else
                    _fileReader = ScriptEngine.Environment.FileOpener.OpenReader(JSONFileName);
            }
            catch (Exception e)
            {
                throw new RuntimeException(e.Message, e);
            }

            _reader = new JsonTextReader(_fileReader);

        }


        /// <summary>
        /// Если текущее значение – начало массива или объекта, то пропускает его содержимое и конец.
        /// Для остальных типов значений работает аналогично методу Прочитать().
        /// </summary>
        ///
        [ContextMethod("Пропустить", "Skip")]
        public bool Skip()
        {

            if (IsOpen())
            {
                if (_reader.TokenType == JsonToken.StartArray || _reader.TokenType == JsonToken.StartObject)
                {
                    while (_reader.Read())
                    {
                        if (_reader.TokenType == JsonToken.EndArray || _reader.TokenType == JsonToken.EndObject)
                        {
                            return _reader.Read();
                        }
                    }
                    return true;
                }
                else
                {
                    if (_reader.Read())
                        return _reader.Read();
                    else
                        return false;
                }

            }
            else
                throw NotOpenException();

        }


        /// <summary>
        /// Выполняет чтение значения JSON.
        /// </summary>
        ///
        [ContextMethod("Прочитать", "Read")]
        public bool Read()
        {
            return _reader.Read();
            
        }


        /// <summary>
        /// 
        /// Устанавливает строку, содержащую текст JSON для чтения данным объектом. Если перед вызовом данного метода уже производилось чтение JSON из другого файла или строки, то чтение прекращается и объект инициализируется для чтения из указанной строки.
        /// </summary>
        ///
        /// <param name="JSONString">
        /// Строка, содержащая текст в формате JSON. </param>
        ///
        ///
        [ContextMethod("УстановитьСтроку", "SetString")]
        public void SetString(string JSONString)
        {
            if (IsOpen())
                Close();

            _reader = new JsonTextReader(new StringReader(JSONString));
        }

    }
}
