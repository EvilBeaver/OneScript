using MessagePack;
using ScriptEngine.Machine;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public struct CommandImage
    {
        [Key(0)]
        public OperationCode Code { get; set; }
        [Key(1)]
        public int Argument { get; set; }
    }
}
