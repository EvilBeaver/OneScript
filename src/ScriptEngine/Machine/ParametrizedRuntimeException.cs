/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;

namespace ScriptEngine.Machine
{
    public class ParametrizedRuntimeException : RuntimeException
    {
        public ParametrizedRuntimeException(string msg, IValue parameter) : base(msg)
        {
            Parameter = parameter;
        }

        public IValue Parameter { get; private set; }
    }
}
