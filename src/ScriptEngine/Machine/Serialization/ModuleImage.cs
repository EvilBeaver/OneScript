using MessagePack;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public class ModuleImage
    {
        [Key(0)]
        public int FormatVersion { get; set; }
        [Key(1)]
        public string SourceHash { get; set; }
        [Key(2)]
        public ConstantImage[] Constants { get; set; }
        [Key(3)]
        public string[] Identifiers { get; set; }
        [Key(4)]
        public CommandImage[] Code { get; set; }
        [Key(5)]
        public MethodImage[] Methods { get; set; }
        [Key(6)]
        public FieldImage[] Fields { get; set; }
        [Key(7)]
        public PropertyImage[] Properties { get; set; }
        [Key(8)]
        public AnnotationDefinitionImage[] ModuleAttributes { get; set; }
        [Key(9)]
        public SymbolicBinding[] VariableBindings { get; set; }
        [Key(10)]
        public SymbolicBinding[] MethodBindings { get; set; }
        [Key(11)]
        public int EntryMethodIndex { get; set; }
    }
}
