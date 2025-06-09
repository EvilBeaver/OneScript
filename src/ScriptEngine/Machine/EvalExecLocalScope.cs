/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Execution;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    internal class EvalExecLocalScope : ScriptDrivenObject, IAttachableContext
    {
        public EvalExecLocalScope(IExecutableModule module) : base(module, false)
        {
        }

        void IAttachableContext.OnAttach(out IVariable[] variables, out BslMethodInfo[] methods)
        {
            variables = Array.Empty<IVariable>();
            methods = Array.Empty<BslMethodInfo>();
        }

        protected override int GetOwnVariableCount() => 0;

        protected override int GetOwnMethodCount() => 0;

        protected override void UpdateState()
        {
        }
    }
}