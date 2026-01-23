using MessagePack;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public struct FieldImage
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public bool IsPublic { get; set; }
        [Key(2)]
        public int DispatchId { get; set; }
        [Key(3)]
        public AnnotationDefinitionImage[] Annotations { get; set; }
    }
}
