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
    public sealed class StructuredValueContainer : SerializableContextBase
    {
        private readonly IEnumerable<Type> _supportedTypes;

        public StructuredValueContainer()
        {
            _supportedTypes = new List<Type>()
            {
                typeof(FixedStructureImpl),
                typeof(StructureImpl),
                typeof(MapImpl),
                typeof(FixedMapImpl)
            };
        }

        [JsonProperty] 
        private List<KeyValuePair<ISerializableContext, ISerializableContext>> Items { get; set; } 
            = new List<KeyValuePair<ISerializableContext, ISerializableContext>>();

        [JsonProperty]
        private Type Type { get; set; }

        public override bool CanProcess(Type type) 
            => _supportedTypes.Contains(type);

        public override IValue GetValue()
        {
            if (Type == typeof(StructureImpl))
                return CreateStruct();
            if (Type == typeof(FixedStructureImpl))
                return CreateFixedStruct();
            if (Type == typeof(MapImpl))
                return CreateMap();
            if (Type == typeof(FixedMapImpl))
                return CreateFixedMap();

            throw new DeserializationException();
        }

        public override void SetValue(IValue value)
        {
            Type = value.GetType();

            var collection = (IEnumerable<KeyAndValueImpl>)value;

            foreach (var item in collection)
            {
                Items.Add(new KeyValuePair<ISerializableContext, ISerializableContext>(
                    InnerSerializer.GetContainer(item.Key),
                    InnerSerializer.GetContainer(item.Value)));
            }
        }

        private FixedMapImpl CreateFixedMap()
        {
            return new FixedMapImpl(CreateMap());
        }

        private MapImpl CreateMap()
        {
            var map = new MapImpl();

            foreach (var item in Items)
                map.Insert(item.Key.GetValue(),
                    item.Value.GetValue());

            return map;
        }

        private FixedStructureImpl CreateFixedStruct()
        {
            return new FixedStructureImpl(CreateStruct());
        }

        private StructureImpl CreateStruct()
        {
            var structure = new StructureImpl();

            foreach (var item in Items)
                structure.Insert(item.Key.GetValue().AsString(),
                    item.Value.GetValue());

            return structure;
        }
    }
}