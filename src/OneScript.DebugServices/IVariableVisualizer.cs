/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using ScriptEngine.Machine;
using Variable = OneScript.DebugProtocol.Variable;

namespace OneScript.DebugServices
{
    public interface IVariableVisualizer
    {
        Variable GetVariable(IVariable value);
        
        IEnumerable<IVariable> GetChildVariables(IValue value);
    }
}