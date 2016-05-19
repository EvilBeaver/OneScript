using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
