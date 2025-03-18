/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Exceptions;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    public class ParametrizedRuntimeException : RuntimeException
    {
        public ParametrizedRuntimeException(string msg, IValue parameter, IValue cause = null) : base(msg)
        {
            Parameter = parameter;
            Cause = cause;
        }

        public IValue Parameter { get; private set; }
        public IValue Cause { get; private set; }
    }
}
