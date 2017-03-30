using System.Runtime.Serialization;

namespace OneScript.DebugProtocol
{
    [DataContract]
    public enum DebugEventType
    {
        [EnumMember]
        BeginExecution,
        [EnumMember]
        ProcessExited
    }
}
