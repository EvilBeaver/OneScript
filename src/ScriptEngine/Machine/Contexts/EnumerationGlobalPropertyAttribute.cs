/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
namespace ScriptEngine
{
    [AttributeUsage (AttributeTargets.Enum)]
    public class EnumerationGlobalPropertyAttribute : Attribute
    {
        public EnumerationGlobalPropertyAttribute (string name, string alias = null)
        {
            Name = name;
            Alias = alias;
        }

        public string Name { get; }
        public string Alias { get; }
    }
}
