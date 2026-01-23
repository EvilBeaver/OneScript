using MessagePack;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public struct MethodSignatureImage
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public string Alias { get; set; }
        [Key(2)]
        public ParameterDefinitionImage[] Params { get; set; }
        [Key(3)]
        public AnnotationDefinitionImage[] Annotations { get; set; }
        [Key(4)]
        public int Flags { get; set; }
    }
}
