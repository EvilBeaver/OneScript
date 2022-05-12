/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace ScriptEngine.Machine
{
    internal class MachineStoredState
    {
        public List<Scope> Scopes { get; set; }
        public Stack<IValue> OperationStack { get; set; }
        public Stack<ExecutionFrame> CallStack { get; set; }
        public Stack<ExceptionJumpInfo> ExceptionsStack { get; set; }
        
        public ICodeStatCollector CodeStatCollector { get; set; }
        
        public MachineStopManager StopManager { get; set; }
        
        public ExecutionContext Memory { get; set; }
    }
}