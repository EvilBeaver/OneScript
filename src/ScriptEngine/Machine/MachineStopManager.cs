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
        private readonly Breakpoints _breakpoints = new Breakpoints();
        private readonly MachineInstance _machine;
        private ExecutionFrame[] _stopFrames;

        
        public int SetBreakpoint(string module, int line)
        {
            return _breakpoints.SetBreakpoint(module, line);
        }

        public MachineStopManager(MachineInstance runner)
        {
            _machine = runner;
        }

        public bool ShouldStopAtThisLine(string module, ExecutionFrame currentFrame)
        {
            bool mustStop = false;
            switch (_currentState)
            {
                case DebugState.Running:
                    mustStop = HitBreakpointOnLine(module, currentFrame);
                    break;
                case DebugState.SteppingIn:
                    throw new NotImplementedException();
                case DebugState.SteppingOut:
                    throw new NotImplementedException();
                case DebugState.SteppingOver:
                    mustStop = StepOverEndsOnFrame(currentFrame);
                    break;
            }

            if(mustStop)
                _currentState = DebugState.Running;

            return mustStop;
            
        }
        
        private bool HitBreakpointOnLine(string module, ExecutionFrame currentFrame)
        {
            return _breakpoints.Find(module, currentFrame.LineNumber);
        }

        private bool StepOverEndsOnFrame(ExecutionFrame currentFrame)
        {
            return _stopFrames != null && _stopFrames.Contains(currentFrame);
        }

        public void ClearBreakpoints()
        {
            _breakpoints.Clear();
        }

        public void StepOver(ExecutionFrame currentFrame)
        {
            _currentState = DebugState.SteppingOver;
            _stopFrames = _machine.GetExecutionFrames().Select(x => x.FrameObject).ToArray();

        }
    }
}
