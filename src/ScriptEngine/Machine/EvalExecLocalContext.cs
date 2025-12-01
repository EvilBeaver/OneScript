/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Execution;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    internal class EvalExecLocalContext : ContextIValueImpl, IAttachableContext
    {
        private readonly IVariable[] _locals;

        public EvalExecLocalContext(IVariable[] locals)
        {
            _locals = locals;
        }

        public void OnAttach(out IVariable[] variables, out BslMethodInfo[] methods)
        {
            throw new NotImplementedException();
        }

        public IVariable GetVariable(int index) => _locals[index];

        public BslMethodInfo GetMethod(int index)
        {
            throw new ArgumentOutOfRangeException();
        }

        int IAttachableContext.VariablesCount => _locals.Length;
        
        int IAttachableContext.MethodsCount => 0;
    }
}