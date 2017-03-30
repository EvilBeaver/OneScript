using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

namespace OneScript.DebugProtocol
{
    [DataContract]
    public class EngineDebugEvent
    {
        [DataMember]
        public DebugEventType EventType;

        public EngineDebugEvent()
        {
            
        }

        public EngineDebugEvent(DebugEventType type)
        {
            EventType = type;
        }

        public string ToSerializedString()
        {
            var ms = new MemoryStream();
            Serialize(ms, this);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static void Serialize(Stream destination, EngineDebugEvent value)
        {
            var ser = new DataContractSerializer(value.GetType());
            var ms = new MemoryStream();
            ser.WriteObject(ms, value);

            var writer = new BinaryWriter(destination, Encoding.UTF8);
            writer.Write(Encoding.UTF8.GetString(ms.ToArray()));
        }

        public static T Deserialize<T>(Stream source) where T : EngineDebugEvent
        {
            var ser = new DataContractSerializer(typeof(T));
            var reader = new BinaryReader(source, Encoding.UTF8);
            var data = reader.ReadString();
            var ms = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(data);
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            return (T)ser.ReadObject(ms);
        }
    }

    
}
