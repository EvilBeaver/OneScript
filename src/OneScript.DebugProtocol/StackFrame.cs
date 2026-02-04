/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace OneScript.DebugProtocol
{
    [DataContract, JsonObject, Serializable]
    public class StackFrame
    {
        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public string MethodName { get; set; }

        [DataMember]
        public int LineNumber { get; set; }

        [DataMember]
        public string Source { get; set; }

        public int ThreadId { get; set; }
    }
}
