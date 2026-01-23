using MessagePack;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public struct AnnotationDefinitionImage
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public AnnotationParameterImage[] Parameters { get; set; }
    }
}
