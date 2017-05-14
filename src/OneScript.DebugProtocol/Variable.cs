using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OneScript.DebugProtocol
{
    [DataContract]
    public class Variable
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsStructured { get; set; }

        [DataMember]
        public string Presentation { get; set; }

        [DataMember]
        public string TypeName { get; set; }

        public int ChildrenHandleID { get; set; }
    }
}
