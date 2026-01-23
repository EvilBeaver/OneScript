using MessagePack;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public struct PropertyImage
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public string Alias { get; set; }
        [Key(2)]
        public bool IsPublic { get; set; }
        [Key(3)]
        public bool CanRead { get; set; }
        [Key(4)]
        public bool CanWrite { get; set; }
        [Key(5)]
        public int DispatchId { get; set; }
        [Key(6)]
        public AnnotationDefinitionImage[] Annotations { get; set; }
    }
}
