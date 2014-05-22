using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    struct Scope
    {
        public IVariable[] Variables;
        public MethodInfo[] Methods;
        public IRuntimeContextInstance Instance;
        public bool Detachable;
    }

}
