using MessagePack;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public struct MethodImage
    {
        [Key(0)]
        public MethodSignatureImage Signature { get; set; }
        [Key(1)]
        public int EntryPoint { get; set; }
        [Key(2)]
        public string[] LocalVariables { get; set; }
    }
}
