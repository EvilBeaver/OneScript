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
    public class ContextClassAttribute : Attribute
    {
        private readonly string _name;
        private readonly string _alias;

        public ContextClassAttribute(string typeName, string typeAlias = "")
        {
            if (!Utils.IsValidIdentifier(typeName))
                throw new ArgumentException("Name must be a valid identifier");

            if (!string.IsNullOrEmpty(typeAlias) && !Utils.IsValidIdentifier(typeAlias))
                throw new ArgumentException("Alias must be a valid identifier");

            _name = typeName;
            _alias = typeAlias;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetAlias()
        {
            return _alias;
        }

        public string TypeUUID { get; set; }

    }
}