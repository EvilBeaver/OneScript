/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Contexts.Enums;

namespace ScriptEngine.Machine.Contexts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SystemEnumAttribute : Attribute, IEnumMetadataProvider
    {
        public SystemEnumAttribute(string name, string alias = "")
        {
            Name = name;
            Alias = alias;
        }

        public string Name { get; }
        public string Alias { get; }
        
        public string TypeUUID { get; set; }
		
        public string ValueTypeUUID { get; set; }
    }
}
