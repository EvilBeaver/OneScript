using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

using Newtonsoft.Json;
using System.Collections.Generic;

namespace OneScript.DebugProtocol
{
    public enum MessageType
    {
        Command,
        Event
    }

    public abstract class DebugProtocolMessage
    {
        public MessageType Type { get; protected set; }
        public string TypeName { get; protected set; }

        protected DebugProtocolMessage()
        {
            TypeName = this.GetType().Name;
        }

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

    public class EngineDebugEvent : DebugProtocolMessage
    {
        public DebugEventType EventType;

        public EngineDebugEvent()
        {
            Type = MessageType.Event;
        }

        public EngineDebugEvent(DebugEventType type) : this()
        {
            EventType = type;
        }
    }

    public class SetSourceBreakpointsCommand : DebugProtocolMessage
    {
        public SetSourceBreakpointsCommand()
        {
            Type = MessageType.Command;
        }

        public Breakpoint[] Breakpoints;
    }
    
}
