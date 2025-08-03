/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
namespace OneScript.DebugProtocol.TcpServer
{
    public class JsonDtoConverter : JsonConverter<TcpProtocolDtoBase>
    {
        private const string TYPE_PROPERTY_NAME = "$type";
        private const string VALUE_PROPERTY_NAME = "$value";

        public override void WriteJson(JsonWriter writer, TcpProtocolDtoBase value, JsonSerializer serializer)
        {
            switch (value)
            {
                case RpcCall rpcCall:
                    WriteCall(writer, rpcCall, serializer);
                    break;
                case RpcCallResult rpcCallResult:
                    WriteCallResult(writer, rpcCallResult, serializer);
                    break;
                default:
                    throw new ArgumentException($"Unknown type {value.GetType()}");
            }
        }

        public override TcpProtocolDtoBase ReadJson(JsonReader reader, Type objectType, TcpProtocolDtoBase existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            CheckExpectedToken(reader, JsonToken.StartObject, $"ReadObject {objectType}");
            
            AdvanceReader(reader);
            var typeName = ReadExpectedProperty<string>(TYPE_PROPERTY_NAME, reader, serializer);
            AdvanceReader(reader);
            
            Type requiredType;
            if (objectType == typeof(TcpProtocolDtoBase))
            {
                // нам не важен тип, прочитать надо тот, который указан в потоке.
                requiredType = typeName switch
                {
                    nameof(RpcCall) => typeof(RpcCall),
                    nameof(RpcCallResult) => typeof(RpcCallResult),
                    _ => throw new JsonSerializationException(
                        $"Unexpected object type in Json. Expected one of {objectType.Name}, got {typeName}")
                };
            }
            else if (typeName != objectType.Name)
            {
                // нам важен тип и он в потоке не такой
                throw new JsonSerializationException($"Unexpected object type in Json. Expected {objectType.Name}, got {typeName}");
            }
            else
            {
                requiredType = objectType;
            }

            TcpProtocolDtoBase readResult;
            if (requiredType == typeof(RpcCall))
            {
                readResult = ReadRpcCall(reader, serializer);
            }
            else if (requiredType == typeof(RpcCallResult))
            {
                readResult = ReadRpcCallResult(reader, serializer);
            }
            else
            {
                throw new InvalidOperationException($"Unknown type {requiredType}"); 
            }
            
            return readResult;
        }

        private RpcCall ReadRpcCall(JsonReader reader, JsonSerializer serializer)
        {
            var value = new RpcCall();
            FillRpcDto(value, reader, serializer, (jsonReader, jsonSerializer, prop) =>
            {
                if (prop == nameof(RpcCall.Parameters))
                {
                    value.Parameters = DeserializeTypedArray(reader, serializer);
                }
            });
            
            return value;
        }
        
        private RpcCallResult ReadRpcCallResult(JsonReader reader, JsonSerializer serializer)
        {
            var value = new RpcCallResult();
            FillRpcDto(value, reader, serializer, (jsonReader, jsonSerializer, prop) =>
            {
                if (prop == nameof(RpcCallResult.ReturnValue))
                {
                    value.ReturnValue = ReadTypedValue(reader, serializer);
                }
            });
            
            return value;
        }

        private void FillRpcDto(
            TcpProtocolDtoBase value,
            JsonReader reader,
            JsonSerializer serializer,
            Action<JsonReader, JsonSerializer, string> readFunction,
            [CallerMemberName] string caller = ""
            )
        {
            bool hasId = false, hasServiceName = false;
            while (reader.TokenType != JsonToken.EndObject)
            {
                CheckExpectedToken(reader, JsonToken.PropertyName, caller);
                
                var propName = (string)reader.Value;
                AdvanceReader(reader);
                switch (propName)
                {
                    case nameof(RpcCallResult.Id):
                        value.Id = serializer.Deserialize<string>(reader);
                        hasId = true;
                        break;
                    case nameof(RpcCallResult.ServiceName):
                        value.ServiceName = serializer.Deserialize<string>(reader);
                        hasServiceName = true;
                        break;
                    default:
                        readFunction(reader, serializer, propName);
                        break;
                }
                AdvanceReader(reader);
            }

            if (!hasId || !hasServiceName)
            {
                throw new JsonSerializationException($"Required properties missing ({caller})");
            }
        }

        private object[] DeserializeTypedArray(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
                
            CheckExpectedToken(reader, JsonToken.StartArray, "TypedArray");
            
            var data = new List<object>();
            while (reader.TokenType != JsonToken.EndArray)
            {
                AdvanceReader(reader);
                if (reader.TokenType == JsonToken.EndArray)
                    break;
                
                var item = ReadTypedValue(reader, serializer);
                data.Add(item);
            }

            return data.ToArray();
        }

        private object ReadTypedValue(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            
            CheckExpectedToken(reader, JsonToken.StartObject, "TypedValue");
            
            AdvanceReader(reader);
            var typeName = ReadExpectedProperty<string>(TYPE_PROPERTY_NAME, reader, serializer);
            AdvanceReader(reader);
            
            var type = GetSupportedType(typeName);
            var value = ReadExpectedProperty(type, VALUE_PROPERTY_NAME, reader, serializer);
            AdvanceReader(reader);

            if (reader.TokenType != JsonToken.EndObject)
            {
                throw new JsonSerializationException("Unexpected token parsing TypedValue " + reader.TokenType);
            }

            return value;
        }

        private void CheckExpectedToken(JsonReader reader, JsonToken expectedToken, string context)
        {
            if (reader.TokenType != expectedToken)
            {
                throw new JsonReaderException($"Unexpected token {reader.TokenType}, ({context}). Expected {expectedToken}");
            }
        }

        private Type GetSupportedType(string typeName)
        {
            bool isArray = false;
            if (typeName.EndsWith("[]", StringComparison.Ordinal))
            {
                isArray = true;
                typeName = typeName.Substring(0, typeName.Length - 2);
            }
            
            var baseType = typeName switch
            {
                nameof(Breakpoint) => typeof(Breakpoint),
                nameof(StackFrame) => typeof(StackFrame),
                nameof(ThreadStopReason) => typeof(ThreadStopReason),
                nameof(Variable) => typeof(Variable),
                nameof(ExceptionBreakpointFilter) => typeof(ExceptionBreakpointFilter),
                nameof(RpcExceptionDto) => typeof(RpcExceptionDto),
                nameof(String) => typeof(string),
                nameof(Int32) => typeof(int),
                nameof(Boolean) => typeof(bool),
                _ => throw new JsonSerializationException($"Unexpected type name for typed value {typeName}")
            };
            
            return isArray ? baseType.MakeArrayType() : baseType; 
        }

        private T ReadExpectedProperty<T>(string propertyName, JsonReader reader, JsonSerializer serializer)
        {
            return (T)ReadExpectedProperty(typeof(T), propertyName, reader, serializer);
        }
        
        private object ReadExpectedProperty(Type targetType, string propertyName, JsonReader reader, JsonSerializer serializer)
        {
            CheckExpectedToken(reader, JsonToken.PropertyName, $"reading expected property {propertyName}");
            
            var propNameInStream = (string)reader.Value;
            if (propNameInStream != propertyName)
            {
                throw new JsonSerializationException($"Unexpected property {propNameInStream}, expected {propertyName}");
            }
            
            AdvanceReader(reader);
            var value = serializer.Deserialize(reader, targetType);
            if (value != null && value.GetType() != targetType)
            {
                throw new JsonSerializationException($"Unexpected object type returned: {value.GetType()}, Expected {targetType}");
            }
            
            return value;
        }

        private static void AdvanceReader(JsonReader reader)
        {
            if (!reader.Read())
            {
                throw new JsonReaderException($"Unexpected end of Json");
            }
        }

        private void WriteCall(JsonWriter writer, RpcCall rpcCall, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            WriteBaseClass(writer, rpcCall);
            writer.WritePropertyName(nameof(rpcCall.Parameters));

            if (rpcCall.Parameters != null)
            {
                writer.WriteStartArray();
                foreach (var callParameter in rpcCall.Parameters)
                {
                    WriteTypedValue(writer, serializer, callParameter);
                }
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteNull();
            }

            writer.WriteEndObject();
        }
        
        private void WriteCallResult(JsonWriter writer, RpcCallResult rpcCallResult, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            WriteBaseClass(writer, rpcCallResult);
            writer.WritePropertyName(nameof(rpcCallResult.ReturnValue));
            WriteTypedValue(writer, serializer, rpcCallResult.ReturnValue);
            
            writer.WriteEndObject();
        }

        private static void WriteTypedValue(JsonWriter writer, JsonSerializer serializer, object valueToWrite)
        {
            if (valueToWrite == null)
            {
                writer.WriteNull();
                return;
            }
            
            writer.WriteStartObject();
                
            writer.WritePropertyName(TYPE_PROPERTY_NAME);
            writer.WriteValue(valueToWrite.GetType().Name);
                
            writer.WritePropertyName(VALUE_PROPERTY_NAME);
            serializer.Serialize(writer, valueToWrite);
                
            writer.WriteEndObject();
        }

        private void WriteBaseClass(JsonWriter writer, TcpProtocolDtoBase dto)
        {
            writer.WritePropertyName(TYPE_PROPERTY_NAME);
            writer.WriteValue(dto.GetType().Name);
            
            writer.WritePropertyName(nameof(dto.Id));
            writer.WriteValue(dto.Id);
            
            writer.WritePropertyName(nameof(dto.ServiceName));
            writer.WriteValue(dto.ServiceName);
        }
    }
}