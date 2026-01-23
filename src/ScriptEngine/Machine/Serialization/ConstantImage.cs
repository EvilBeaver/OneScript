using MessagePack;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public struct ConstantImage
    {
        [Key(0)]
        public ConstantKind Kind { get; set; }
        [Key(1)]
        public string StringValue { get; set; }
        [Key(2)]
        public decimal NumericValue { get; set; }
        [Key(3)]
        public bool BooleanValue { get; set; }
        [Key(4)]
        public long DateTicks { get; set; }
        [Key(5)]
        public string TypeName { get; set; }
    }

    public enum ConstantKind
    {
        Undefined,
        Null,
        Boolean,
        Number,
        String,
        Date,
        Type,
        Annotation,
        SkippedParameter
    }
}
