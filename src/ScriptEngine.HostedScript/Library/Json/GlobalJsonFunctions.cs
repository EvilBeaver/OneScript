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
            if (ReadToMap)
                return ReadJSONInMap(Reader);
            else
                return ReadJSONInStruct(Reader);
        }

        public IValue ReadJSONInStruct(JSONReader Reader, bool nestedArray = false)
        {
            if (nestedArray)
            {
                ArrayImpl NestedArray = new ArrayImpl();

                while (Reader.Read())
                {
                    if (Reader.CurrentJsonTokenType == JsonToken.EndArray)
                    {
                        break;
                    }
                    else if (Reader.CurrentJsonTokenType == JsonToken.StartObject)
                    {
                        NestedArray.Add(ReadJSONInStruct(Reader));
                    }
                    else if (Reader.CurrentJsonTokenType == JsonToken.StartArray)
                    {
                        NestedArray.Add(ReadJSONInStruct(Reader, true));
                    }
                    else
                        NestedArray.Add(Reader.CurrentValue);
                }
                return NestedArray;
            }

            StructureImpl ResStruct = new StructureImpl();

            while ((Reader).Read())
            {
                if (Reader.CurrentJsonTokenType == JsonToken.PropertyName)
                {
                    string PropertyName = Reader.CurrentValue.AsString();
                    Reader.Read();

                    if (Reader.CurrentJsonTokenType == JsonToken.StartObject)
                    {
                        ResStruct.Insert(PropertyName, ReadJSONInStruct(Reader));
                    }
                    else if (Reader.CurrentJsonTokenType == JsonToken.StartArray)
                    {
                        ResStruct.Insert(PropertyName, ReadJSONInStruct(Reader, true));
                    }
                    else
                    {
                        ResStruct.Insert(PropertyName, Reader.CurrentValue);
                    }
                }
                else if (Reader.CurrentJsonTokenType == JsonToken.EndObject)
                {
                    break;
                }
                else if (Reader.CurrentJsonTokenType == JsonToken.StartArray)
                {
                    return ReadJSONInStruct(Reader, true);
                }
            }
            return ResStruct;
        }

        public IValue ReadJSONInMap(JSONReader Reader, bool nestedArray = false)
        {

            if (nestedArray)
            {
                ArrayImpl NestedArray = new ArrayImpl();

                while (Reader.Read())
                {
                    if (Reader.CurrentJsonTokenType == JsonToken.EndArray)
                    {
                        break;
                    }
                    else if (Reader.CurrentJsonTokenType == JsonToken.StartObject)
                    {
                        NestedArray.Add(ReadJSONInMap(Reader));
                    }
                    else if (Reader.CurrentJsonTokenType == JsonToken.StartArray)
                    {
                        NestedArray.Add(ReadJSONInMap(Reader, true));
                    }
                    else
                        NestedArray.Add(Reader.CurrentValue);
                }
                return NestedArray;
            }

            MapImpl ResStruct = new MapImpl();
            while ((Reader).Read())
            {
                if (Reader.CurrentJsonTokenType == JsonToken.PropertyName)
                {
                    string PropertyName = Reader.CurrentValue.AsString();
                    Reader.Read();

                    if (Reader.CurrentJsonTokenType == JsonToken.StartObject)
                    {
                        ResStruct.Insert(ValueFactory.Create(PropertyName), ReadJSONInMap(Reader));
                    }
                    else if (Reader.CurrentJsonTokenType == JsonToken.StartArray)
                    {
                        ResStruct.Insert(ValueFactory.Create(PropertyName), ReadJSONInMap(Reader, true));
                    }
                    else
                    {
                        ResStruct.Insert(ValueFactory.Create(PropertyName), Reader.CurrentValue);
                    }
                }
                else if (Reader.CurrentJsonTokenType == JsonToken.EndObject)
                {
                    break;
                }
                else if (Reader.CurrentJsonTokenType == JsonToken.StartArray)
                {
                    return ReadJSONInMap(Reader, true);
                }
            }
            return ResStruct;
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
                throw new RuntimeException("Формат даты JavaScript не поддерживается.");

            string json = @"{""Date"":""" + String +  @"""}";

            var settings = new JsonSerializerSettings
            {
                DateFormatHandling = dateFormatHandling
        };
            var result = JsonConvert.DeserializeObject<ConvertedDate>(json, settings);
            return ValueFactory.Create((DateTime)result.Date);
        }

        /// <summary>
        /// 
        /// Выполняет сериализацию Значение в формат JSON. Результат помещает в объект ЗаписьJSON.
        /// Если методу требуется передать значение недопустимого типа, то можно использовать функцию преобразования значения (параметры ИмяФункцииПреобразования и МодульФункцииПреобразования).
        /// </summary>
        ///
        /// <param name="JSONWriter">
        /// Объект, через который осуществляется запись JSON. Поток JSON должен быть подготовлен для записи значения. </param>
        /// <param name="Value">
        /// Объект записи JSON. Меняет состояние потока записи. 
        /// <param name="SerializationSettings">
        /// В текущий версии не обрабатывается. Настройки сериализации в JSON. </param>
        /// <param name="ConversionFunctionName">
        /// В текущий версии не обрабатывается. Значение по умолчанию: Неопределено. </param>
        /// <param name="ConversionFunctionModule">
        /// Указывает контекст, в котором реализована функция преобразования значения в значение формата JSON.
        ///В текущий версии не обрабатывается.  Значение по умолчанию: Неопределено. </param>
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
                    if (item is StructureImpl || item is MapImpl || item is FixedStructureImpl || item is FixedMapImpl)
                    {
                        WriteJSON(Writer, item);
                    }
                    else if (item is ArrayImpl || item is FixedArrayImpl)
                    {
                        WriteJSON(Writer, item);
                    }
                    else
                        Writer.WriteValue(item);
                }
                Writer.WriteEndArray();
            }
            else if (RawValue is FixedArrayImpl)
            {
                Writer.WriteStartArray();
                foreach (var item in (FixedArrayImpl)RawValue)
                {
                    if (item is StructureImpl || item is MapImpl || item is FixedStructureImpl || item is FixedMapImpl)
                    {
                        WriteJSON(Writer, item);
                    }
                    else if (item is ArrayImpl || item is FixedArrayImpl)
                    {
                        WriteJSON(Writer, item);
                    }
                    else
                        Writer.WriteValue(item);
                }
                Writer.WriteEndArray();
            }
            else if (RawValue is StructureImpl)
            {
                Writer.WriteStartObject();
                foreach (var item in (StructureImpl)RawValue)
                {
                    if (item.Value is StructureImpl || item.Value is MapImpl || item.Value is FixedStructureImpl || item.Value is FixedMapImpl || item.Value is ArrayImpl || item.Value is FixedArrayImpl)
                    {
                        Writer.WritePropertyName(item.Key.AsString());
                        WriteJSON(Writer, item.Value);
                    }
                    else { 
                        Writer.WritePropertyName(item.Key.AsString());
                        Writer.WriteValue(item.Value);
                    }
                }
                Writer.WriteEndObject();
            }
            else if (RawValue is FixedStructureImpl)
            {
                Writer.WriteStartObject();
                foreach (var item in (FixedStructureImpl)RawValue)
                {
                    if (item.Value is StructureImpl || item.Value is MapImpl || item.Value is FixedStructureImpl || item.Value is FixedMapImpl || item.Value is ArrayImpl || item.Value is FixedArrayImpl)
                    {
                        Writer.WritePropertyName(item.Key.AsString());
                        WriteJSON(Writer, item.Value);
                    }
                    else
                    {
                        Writer.WritePropertyName(item.Key.AsString());
                        Writer.WriteValue(item.Value);
                    }
                }
                Writer.WriteEndObject();
            }
            else if (RawValue is MapImpl)
            {
                Writer.WriteStartObject();
                foreach (var item in (MapImpl)RawValue)
                {
                    if (item.Value is StructureImpl || item.Value is MapImpl || item.Value is FixedStructureImpl || item.Value is FixedMapImpl || item.Value is ArrayImpl || item.Value is FixedArrayImpl)
                    {
                        Writer.WritePropertyName(item.Key.AsString());
                        WriteJSON(Writer, item.Value);
                    }
                    else
                    {
                        Writer.WritePropertyName(item.Key.AsString());
                        Writer.WriteValue(item.Value);
                    }
                }
                Writer.WriteEndObject();
            }
            else if (RawValue is FixedMapImpl)
            {
                Writer.WriteStartObject();
                foreach (var item in (FixedMapImpl)RawValue)
                {
                    if (item.Value is StructureImpl || item.Value is MapImpl || item.Value is FixedStructureImpl || item.Value is FixedMapImpl || item.Value is ArrayImpl || item.Value is FixedArrayImpl)
                    {
                        Writer.WritePropertyName(item.Key.AsString());
                        WriteJSON(Writer, item.Value);
                    }
                    else
                    {
                        Writer.WritePropertyName(item.Key.AsString());
                        Writer.WriteValue(item.Value);
                    }
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
