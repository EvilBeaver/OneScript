/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ScriptEngine.HostedScript.Library.Json
{

    /// <summary>
    /// Глобальный контекст. Операции с Json.
    /// </summary>
    [GlobalContext(Category = "Процедуры и функции работы с JSON")]
    public sealed class GlobalJsonFunctions : GlobalContextBase<GlobalJsonFunctions>
    {
        public static IAttachableContext CreateInstance()
        {
            return new GlobalJsonFunctions();
        }

        /// <summary>
        /// 
        /// Считывает значение из JSON-текста или файла. JSON-текст должен быть корректным.
        /// </summary>
        ///
        /// <param name="Reader">
        /// Объект чтения JSON.</param>
        /// <param name="ReadToMap">
        /// Если установлено Истина, чтение объекта JSON будет выполнено в Соответствие.
        /// Если установлено Ложь, объекты будут считываться в объект типа Структура.
        /// Значение по умолчанию: Ложь. </param>
        /// <param name="PropertiesWithDateValuesNames">
        /// Значение не обрабатывается в текущей версии. Значение по умолчанию: Неопределено.</param>
        /// <param name="ExpectedDateFormat">
        /// Значение не обрабатывается в текущей версии. Значение по умолчанию: ISO. </param>
        /// <param name="ReviverFunctionName">
        /// Значение не обрабатывается в текущей версии. Значение по умолчанию: Неопределено. </param>
        /// <param name="ReviverFunctionModule">
        /// Значение не обрабатывается в текущей версии. Значение по умолчанию: Неопределено.</param>
        /// <param name="ReviverFunctionAdditionalParameters">
        /// Значение не обрабатывается в текущей версии. Значение по умолчанию: Неопределено. </param>
        /// <param name="RetriverPropertiesNames">
        /// Значение не обрабатывается в текущей версии. Значение по умолчанию: Неопределено. </param>
        /// <param name="MaximumNesting">
        /// Значение не обрабатывается в текущей версии.</param>
        ///
        /// <returns name="Structure, Map или Array"></returns>
        ///
        [ContextMethod("ПрочитатьJSON", "ReadJSON")]
        public IValue ReadJSON(JSONReader Reader, bool ReadToMap = false, IValue PropertiesWithDateValuesNames = null, IValue ExpectedDateFormat = null, string ReviverFunctionName = null, IValue ReviverFunctionModule = null, IValue ReviverFunctionAdditionalParameters = null, IValue RetriverPropertiesNames = null, int MaximumNesting = 500)
        {
            var jsonReader = new JsonReaderInternal(Reader);
            return ReadToMap ? jsonReader.Read<MapImpl>() : jsonReader.Read<StructureImpl>();
        }

        internal class JsonReaderInternal
        {
            private readonly JSONReader _reader;
            private Func<IValue> _builder;
            private Action<IValue, string, IValue> _inserter;

            private void Init<TStructure>()
            {
                if (typeof(TStructure) == typeof(StructureImpl))
                {
                    _builder = () => new StructureImpl();
                    _inserter = (o, s, v) => ((StructureImpl)o).Insert(s, v);
                }
                else if (typeof(TStructure) == typeof(MapImpl))
                {
                    _builder = () => new MapImpl();
                    _inserter = (o, s, v) =>((MapImpl)o).Insert(ValueFactory.Create(s), v);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            public JsonReaderInternal(JSONReader reader)
            {
                _reader = reader;
            }

            private IValue Create() => _builder();

            private void AddProperty(IValue obj, string str, IValue val) => _inserter(obj, str, val);
            
            public IValue Read<T>() where T: IEnumerable<KeyAndValueImpl>
            {
                System.Diagnostics.Debug.Assert(typeof(T)==typeof(StructureImpl)||typeof(T)==typeof(MapImpl));
                Init<T>();

                try
                {
                    if (ReadJsonValue(out var value))
                        return value;
                }
                catch (JSONReaderException)
                {
                    throw;
                }
                catch (Exception exc)
                {
                    throw InvalidJsonException(exc.Message);
                }

                throw InvalidJsonException();
            }

            private JsonToken ReadJsonToken()
            {
                while (_reader.Read())
                {
                    var tok = _reader.CurrentJsonTokenType;
                    if (tok != JsonToken.Comment)
                        return tok;
                }

                return JsonToken.None;
            }
 
            private bool ReadJsonValue(out IValue value) 
            {
                switch (ReadJsonToken())
                {
                    case JsonToken.StartObject:
                        var jsonObject = Create();

                        while (ReadJsonToken() == JsonToken.PropertyName)
                        {
                            var propertyName = _reader.CurrentValue.AsString();
                            if (!ReadJsonValue(out value))
                                return false;

                            AddProperty(jsonObject, propertyName, value);
                        }

                        if (_reader.CurrentJsonTokenType == JsonToken.EndObject)
                        {
                            value = jsonObject;
                            return true;
                        }
                        break;

                    case JsonToken.StartArray:
                        var resArray = new ArrayImpl();

                        while (ReadJsonValue(out value))
                        {
                            resArray.Add(value);
                        }

                        if (_reader.CurrentJsonTokenType == JsonToken.EndArray)
                        {
                            value = resArray;
                            return true;
                        }
                        break;

                    case JsonToken.EndArray:
                    case JsonToken.EndObject:
                    case JsonToken.None:
                        break;

                    default:
                        value = _reader.CurrentValue;
                        return true;
                }

                value = null;
                return false;
            }

            private RuntimeException InvalidJsonException(string message=null)
            {
                var addition = string.IsNullOrWhiteSpace(message) ? string.Empty : $"\n({message})";
                return new RuntimeException(string.Format(Locale.NStr
                    ("ru='Недопустимое состояние потока чтения JSON в строке {0} позиции {1}{2}'"
                     + "en='Invalid JSON reader state at line {0} position {1}{2}'"),
                    _reader.CurrentLine, _reader.CurrentPosition, addition));
            }
        }


        /// <summary>
        /// 
        /// Выполняет преобразование строки, прочитанной в JSON-формате, в значение типа Дата.
        /// </summary>
        ///
        /// <param name="String">
        /// Строка, которую требуется преобразовать в дату. </param>
        /// <param name="Format">
        /// Формат, в котором представлена дата в строке, подлежащей преобразованию. </param>
        ///
        /// <returns name="Date">
        /// Значения данного типа содержит дату григорианского календаря (с 01 января 0001 года) и время с точностью до секунды.</returns>
        ///
        [ContextMethod("ПрочитатьДатуJSON", "ReadJSONDate")]
        public IValue ReadJSONDate(string String, IValue Format)
        {

            var format = Format.GetRawValue() as SelfAwareEnumValue<JSONDateFormatEnum>;
            var JSONDateFormatEnum = GlobalsManager.GetEnum<JSONDateFormatEnum>();
            DateFormatHandling dateFormatHandling = new DateFormatHandling();

            if (format == JSONDateFormatEnum.ISO || format == null)
                dateFormatHandling = DateFormatHandling.IsoDateFormat;
            else if (format == JSONDateFormatEnum.Microsoft)
                dateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            else
                throw new RuntimeException(Locale.NStr("ru='Формат даты JavaScript не поддерживается.'; en='JavaScript date format is not supported'"));

            string json = @"{""Date"":""" + String + @"""}";

            var settings = new JsonSerializerSettings
            {
                DateFormatHandling = dateFormatHandling
            };

            try
            {
                var result = JsonConvert.DeserializeObject<ConvertedDate>(json, settings);
                return ValueFactory.Create((DateTime)result.Date);
            }
            catch (JsonException)
            {
                throw new RuntimeException(Locale.NStr("ru='Представление даты имеет неверный формат.'; en='Invalid date presentation format'"));
            }
        }

        /// <summary>
        /// 
        /// Выполняет сериализацию Значение в формат JSON. Результат помещает в объект ЗаписьJSON.
        /// Если методу требуется передать значение недопустимого типа, то можно использовать функцию преобразования значения (параметры ИмяФункцииПреобразования и МодульФункцииПреобразования).
        /// </summary>
        ///
        /// <param name="Writer">
        /// Объект, через который осуществляется запись JSON. Поток JSON должен быть подготовлен для записи значения. </param>
        /// <param name="Value">
        /// Объект записи JSON. Меняет состояние потока записи. </param>
        /// <param name="SerializationSettings">
        /// В текущий версии не обрабатывается. Настройки сериализации в JSON. </param>
        /// <param name="ConversionFunctionName">
        /// В текущий версии не обрабатывается. Значение по умолчанию: Неопределено. </param>
        /// <param name="ConversionFunctionModule">
        /// Указывает контекст, в котором реализована функция преобразования значения в значение формата JSON.
        /// В текущий версии не обрабатывается.  Значение по умолчанию: Неопределено. </param>
        /// <param name="ConversionFunctionAdditionalParameters">
        /// В текущий версии не обрабатывается. Значение по умолчанию: Неопределено. </param>
        ///
        ///
        [ContextMethod("ЗаписатьJSON", "WriteJSON")]
        public void WriteJSON(JSONWriter Writer, IValue Value, IValue SerializationSettings = null, string ConversionFunctionName = null, IValue ConversionFunctionModule = null, IValue ConversionFunctionAdditionalParameters = null)
        {

            var RawValue = Value.GetRawValue();

            if (RawValue is ArrayImpl)
            {
                Writer.WriteStartArray();
                foreach (var item in (ArrayImpl)RawValue)
                {
                        WriteJSON(Writer, item);
                }
                Writer.WriteEndArray();
            }
            else if (RawValue is FixedArrayImpl)
            {
                Writer.WriteStartArray();
                foreach (var item in (FixedArrayImpl)RawValue)
                {
                        WriteJSON(Writer, item);
                }
                Writer.WriteEndArray();
            }
            else if (RawValue is StructureImpl)
            {
                Writer.WriteStartObject();
                foreach (var item in (StructureImpl)RawValue)
                {
                        Writer.WritePropertyName(item.Key.AsString());
                        WriteJSON(Writer, item.Value);
                }
                Writer.WriteEndObject();
            }
            else if (RawValue is FixedStructureImpl)
            {
                Writer.WriteStartObject();
                foreach (var item in (FixedStructureImpl)RawValue)
                {
                        Writer.WritePropertyName(item.Key.AsString());
                        WriteJSON(Writer, item.Value);
                }
                Writer.WriteEndObject();
            }
            else if (RawValue is MapImpl)
            {
                Writer.WriteStartObject();
                foreach (var item in (MapImpl)RawValue)
                {
                        Writer.WritePropertyName(item.Key.AsString());
                        WriteJSON(Writer, item.Value);
                }
                Writer.WriteEndObject();
            }
            else if (RawValue is FixedMapImpl)
            {
                Writer.WriteStartObject();
                foreach (var item in (FixedMapImpl)RawValue)
                {
                       Writer.WritePropertyName(item.Key.AsString());
                       WriteJSON(Writer, item.Value);
                }
                Writer.WriteEndObject();
            }
            else
                Writer.WriteValue(RawValue);
        }
     }

    class ConvertedDate
    {
        public DateTime Date { get; set; }
    }

}
