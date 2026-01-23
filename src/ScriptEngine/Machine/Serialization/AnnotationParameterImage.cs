using MessagePack;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public struct AnnotationParameterImage
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public ConstantImage Value { get; set; }
    }
}
