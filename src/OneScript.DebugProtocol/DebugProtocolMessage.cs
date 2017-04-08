using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OneScript.DebugProtocol
{
    public class DebugProtocolMessage
    {
        public string Name { get; set; }
        public object Data { get; set; }


        public static void Serialize(Stream destination, DebugProtocolMessage value)
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            var jsonString = JsonConvert.SerializeObject(value, settings);
            var writer = new BinaryWriter(destination, Encoding.UTF8);
            writer.Write(jsonString);
        }

        public static T Deserialize<T>(Stream source) where T : DebugProtocolMessage
        {
            var reader = new BinaryReader(source, Encoding.UTF8);
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            var obj = JsonConvert.DeserializeObject<T>(reader.ReadString(), settings);

            return obj;
        }

        public string ToSerializedString()
        {
            var ms = new MemoryStream();
            Serialize(ms, this);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
