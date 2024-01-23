/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;

namespace ScriptEngine.Machine.Interfaces
{
    public class IteratorBslInterface
    {
        public IteratorBslInterface(BslScriptMethodInfo moveNextMethod, BslScriptMethodInfo getCurrentMethod, BslScriptMethodInfo onDisposeMethod)
        {
            MoveNextMethod = moveNextMethod;
            GetCurrentMethod = getCurrentMethod;
            OnDisposeMethod = onDisposeMethod;
        }
        
        public BslScriptMethodInfo MoveNextMethod { get; }
        public BslScriptMethodInfo GetCurrentMethod { get; }
        public BslScriptMethodInfo OnDisposeMethod { get; }
    }
}