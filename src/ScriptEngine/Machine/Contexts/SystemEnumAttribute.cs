/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;

namespace ScriptEngine.Machine.Contexts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SystemEnumAttribute : Attribute
    {
        private readonly string _name;
        private readonly string _alias;

        public SystemEnumAttribute(string name, string alias = "")
        {
            _name = name;
            _alias = alias;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetAlias()
        {
            return _alias;
        }
    }
}
