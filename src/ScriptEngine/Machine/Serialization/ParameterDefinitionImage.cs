using MessagePack;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public struct ParameterDefinitionImage
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public bool IsByValue { get; set; }
        [Key(2)]
        public bool HasDefaultValue { get; set; }
        [Key(3)]
        public int DefaultValueIndex { get; set; }
        [Key(4)]
        public AnnotationDefinitionImage[] Annotations { get; set; }
    }
}
