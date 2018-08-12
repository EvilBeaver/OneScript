/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Newtonsoft.Json;

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

    }
}
