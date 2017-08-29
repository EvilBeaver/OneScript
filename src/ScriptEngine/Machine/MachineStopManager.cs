/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ScriptEngine.Environment;

namespace ScriptEngine.Machine
{

    internal enum DebugState
    {
        Running,
        SteppingOver,
        SteppingIn,
        SteppingOut
    }

    

    internal class MachineStopManager
    {
        private DebugState _currentState = DebugState.Running;
        private Breakpoints _breakpoints = new Breakpoints();
        private ExecutionFrame _stopFrame;
        
        public int SetBreakpoint(string module, int line)
        {
            return _breakpoints.SetBreakpoint(module, line);
        }
        
        public bool ShouldStopAtThisLine(string module, ExecutionFrame currentFrame)
        {
            switch (_currentState)
            {
                case DebugState.Running:
                    return HitBreakpointOnLine(module, currentFrame);
                case DebugState.SteppingIn:
                    throw new NotImplementedException();
                case DebugState.SteppingOut:
                    throw new NotImplementedException();
                case DebugState.SteppingOver:
                    throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        private bool HitBreakpointOnLine(string module, ExecutionFrame currentFrame)
        {
            return _breakpoints.Find(module, currentFrame.LineNumber);
        }

        public void ClearBreakpoints()
        {
            _breakpoints.Clear();
        }
    }
}
