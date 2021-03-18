/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Runtime.CompilerServices;
using ScriptEngine.Machine;

namespace ScriptEngine.Types
{
    public struct TypeActivationContext
    {
        public string TypeName { get; set; }
        
        public MachineEnvironment MachineEnvironment { get; set; }

        public ITypeManager TypeManager => MachineEnvironment.TypeManager;

        public IGlobalsManager GlobalsManager => MachineEnvironment.GlobalInstances;

        public RuntimeEnvironment GlobalNamespace => MachineEnvironment.GlobalNamespace;
    }
}