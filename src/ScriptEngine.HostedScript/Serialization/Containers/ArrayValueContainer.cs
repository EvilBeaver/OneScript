/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Serialization.Containers
{
    public class ArrayValueContainer : SerializableContextBase
    {
        private readonly IEnumerable<Type> _supportedTypes = new List<Type>()
        {
            typeof(ArrayImpl),
            typeof(FixedArrayImpl)
        };

        [JsonProperty]
        private List<ISerializableContext> Values { get; set; }

        [JsonProperty]
        private Type Type { get; set; }

        public ArrayValueContainer()
        {
            Values = new List<ISerializableContext>();
        }

        public override bool CanProcess(Type type) 
            => _supportedTypes.Contains(type);

        public override IValue GetValue()
        {
            var array = new ArrayImpl();
         
            foreach (var value in Values)
                array.Add(value.GetValue());

            if (Type == typeof(FixedArrayImpl))
                return new FixedArrayImpl(array);

            return array;
        }

        public override void SetValue(IValue value)
        {
            Type = value.GetType();

            if (!(value is IEnumerable<IValue> collection))
                throw new SerializationException();
          
            foreach (var item in collection)
                Values.Add(InnerSerializer.GetContainer(item));
        }
    }
}