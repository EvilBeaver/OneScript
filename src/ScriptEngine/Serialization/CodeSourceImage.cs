using System.Collections.Generic;
using MessagePack;

namespace ScriptEngine.Serialization
{
    [MessagePackObject]
    public sealed class CodeSourceImage
    {
        [Key(0)]
        public string ProviderKey { get; set; }
        
        [Key(1)]
        public string Location { get; set; }
        
        [Key(2)]
        public string Name { get; set; }
        
        [Key(3)]
        public string OwnerPackageId { get; set; }
        
        [Key(4)]
        public Dictionary<string, string> Metadata { get; set; }
    }
}
