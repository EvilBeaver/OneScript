/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Machine
{
    public class MachineEnvironment
    {
        public MachineEnvironment(ITypeManager typeManager, RuntimeEnvironment globalNamespace, IGlobalsManager globalInstances)
        {
            TypeManager = typeManager;
            GlobalNamespace = globalNamespace;
            GlobalInstances = globalInstances;
        }
        
        public ITypeManager TypeManager { get; }
        
        public RuntimeEnvironment GlobalNamespace { get; }
        
        public IGlobalsManager GlobalInstances { get; }
    }
}