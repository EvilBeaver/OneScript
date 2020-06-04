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
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Serialization
{
    public static class InnerSerializer
    {
        private static readonly Lazy<IEnumerable<ISerializableContext>> Serializers 
            = new Lazy<IEnumerable<ISerializableContext>>(LoadSerializers);

        public static ISerializableContext GetContainer(IValue rawValue)
        {
            IValue value = rawValue;

            if (value is Variable variable)
                value = variable.Value;

            var valueType = value.GetType();

            var container = Serializers.Value.FirstOrDefault(x => x.CanProcess(valueType));

            if (container == null)
                throw new SerializationException();

            container.SetValue(value);

            return container;
        }

        public static string Serialize(IValue rawValue)
        {
            var container = GetContainer(rawValue);

            return JsonConvert.SerializeObject(container, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            });
        }

        public static IValue Deserialize(string json)
        {
            var container = (ISerializableContext)JsonConvert.DeserializeObject(json, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return container.GetValue();
        }

        private static IEnumerable<ISerializableContext> LoadSerializers()
        {
            var baseType = typeof(ISerializableContext);

            var serializersTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => baseType.IsAssignableFrom(x) && !x.IsAbstract)
                .ToList()
                .Select(x => (ISerializableContext) Activator.CreateInstance(x));

            return serializersTypes;
        }
    }
}