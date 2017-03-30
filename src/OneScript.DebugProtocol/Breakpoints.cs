using System.Runtime.Serialization;

namespace OneScript.DebugProtocol
{
    public class Breakpoint
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public int Line { get; set; }
    }
    
}
