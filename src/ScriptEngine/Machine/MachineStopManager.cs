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
        //public ExecutionFrame HitFrame;
        public int LineNumber;

        public Breakpoint(int id)
        {
            BreakpointId = id;
        }

        public int BreakpointId { get; }
    }


    internal class MachineStopManager
    {
        private DebugState _currentState = DebugState.Running;
        private List<Breakpoint> _breakpoints = new List<Breakpoint>();

        private ExecutionFrame _stopFrame;

        private int _bpIdsGenerator;

        public int SetBreakpoint(string module, int line)
        {
            var bp = new Breakpoint(_bpIdsGenerator++)
            {
                LineNumber = line,
                Module = module
            };

            _breakpoints.Add(bp);
            return bp.BreakpointId;
        }

        public void RemoveBreakpoint(int bpId)
        {
            int index = _breakpoints.FindIndex(x => x.BreakpointId == bpId);
            if(index >= 0)
                _breakpoints.RemoveAt(index);
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
            var found = _breakpoints.Find(x => x.Module.Equals(module) && x.LineNumber == currentFrame.LineNumber);
            return found != null;
        }

    }
}
