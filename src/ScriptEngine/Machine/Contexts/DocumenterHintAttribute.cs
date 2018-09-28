/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;


namespace ScriptEngine.Machine.Contexts
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Enum)]
    public class DocumenterHintAttribute : Attribute
    {
        public string Name { get; set; }

        public string Alias { get; set; }

        public ContextKind Kind { get; set; }
    }

    public enum ContextKind
    {
        Class,
        GlobalContext,
        Enumeration
    }
}
