using MessagePack;
using ScriptEngine.Machine;
using OneScript.Compilation.Binding;

namespace ScriptEngine.Machine.Serialization
{
    [MessagePackObject]
    public struct SymbolicBinding
    {
        [Key(0)]
        public ScopeBindingKind Kind { get; set; }
        [Key(1)]
        public int ScopeIndex { get; set; }
        [Key(2)]
        public int MemberNumber { get; set; }
        [Key(3)]
        public string ContextTypeName { get; set; }
        [Key(4)]
        public string SymbolName { get; set; }
    }
}
