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

    internal class Breakpoint
    {
        public string Module;
        public ExecutionFrame HitFrame;
        public int LineNumber;

        public Breakpoint(int id)
        {
            BreakpointId = id;
        }

        public int BreakpointId { get; }
    }


    internal class MachineStopManager
    {
        private DebugState _currentState;
        private List<Breakpoint> _breakpoints;

        private int _bpIdsGenerator;

        public int SetBreakpoint(string module, int line)
        {
            return (new Breakpoint(_bpIdsGenerator++)
            {
                LineNumber = line,
                Module = module
            }).BreakpointId;
        }

        public void RemoveBreakpoint(int bpId)
        {
            int index = _breakpoints.FindIndex(x => x.BreakpointId == bpId);
            if(index >= 0)
                _breakpoints.RemoveAt(index);
        }

        public bool LineHit(string module, ExecutionFrame currentFrame)
        {
            switch (_currentState)
            {
                case DebugState.Running:
                    throw new NotImplementedException();
                case DebugState.SteppingIn:
                    throw new NotImplementedException();
                case DebugState.SteppingOut:
                    throw new NotImplementedException();
                case DebugState.SteppingOver:
                    throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }
    }
}
