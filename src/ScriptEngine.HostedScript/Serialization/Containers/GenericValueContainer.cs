/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using Newtonsoft.Json;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Values;

namespace ScriptEngine.HostedScript.Serialization.Containers
{
    public class GenericValueContainer : SerializableContextBase<GenericValue>
    {
        [JsonProperty]
        private object Value { get; set; }
        
        [JsonProperty]
        private DataType DataType { get; set; }

        public override void SetValue(GenericValue value)
        {
            DataType = value.DataType;
            
            switch (value.DataType)
            {
                case DataType.Boolean:
                    Value = value.AsBoolean();
                    break;
                case DataType.Date:
                    Value = value.AsDate();
                    break;
                case DataType.Number:
                    Value = value.AsNumber();
                    break;
                case DataType.String:
                    Value = value.AsString();
                    break;
                default:
                    throw new SerializationException();
            }
        }
        
        public override GenericValue GetValue()
        {
            IValue value;
            
            switch (DataType)
            {
                case DataType.Boolean:
                    value = ValueFactory.Create((bool) Value);
                    break;
                case DataType.Date:
                    value = ValueFactory.Create((DateTime) Value);
                    break;
                case DataType.Number:
                    value = ValueFactory.Create(Convert.ToDecimal(Value));
                    break;
                case DataType.String:
                    value = ValueFactory.Create((string) Value);
                    break;
                default:
                    throw new DeserializationException();
            }

            return (GenericValue) value;

        }
    }
}