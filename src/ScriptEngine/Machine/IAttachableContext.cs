/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Machine
{
    public interface IAttachableContext : IRuntimeContextInstance
    {
        void OnAttach(MachineInstance machine,
                      out IVariable[] variables,
                      out MethodInfo[] methods);
    }

    internal interface IRunnable : IAttachableContext
    {
        LoadedModule Module { get; }
    }
}
