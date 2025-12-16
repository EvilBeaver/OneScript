/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Commons;

namespace ScriptEngine.Machine
{
    public struct AnnotationParameter
    {
        public string Name;

        public IValue RuntimeValue;

        public override readonly string ToString()
        {
            return Utils.NameAndValuePresentation(Name, RuntimeValue);
        }

    }
}
