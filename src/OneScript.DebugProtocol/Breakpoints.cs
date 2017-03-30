using System.Runtime.Serialization;

namespace OneScript.DebugProtocol
{
    [DataContract]
    public class Breakpoint
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public int Line { get; set; }
    }
    
}
