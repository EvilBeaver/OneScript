/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;

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
        private struct StopPoint
        {
            public ExecutionFrame frame;
            public int line;
        }

        private DebugState _currentState = DebugState.Running;
        private readonly Breakpoints _breakpoints = new Breakpoints();
        private readonly MachineInstance _machine;
        private ExecutionFrame[] _stopFrames;

        private StopPoint _lastStopPoint;

        public Breakpoints Breakpoints => _breakpoints;
        
        public MachineStopReason LastStopReason { get; internal set; }

        public DebugState CurrentState => _currentState;

        public int SetBreakpoint(string module, int line)
        {
            return _breakpoints.SetBreakpoint(module, line);
        }

        internal int RemoveBreakpoint(string source, int line)
        {
            var id = _breakpoints.FindIndex(source, line);
            if(id > 0)
            {
                _breakpoints.RemoveBreakpoint(id);
            }

            return id;

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
                    mustStop = true;
                    break;
                case DebugState.SteppingOut:
                case DebugState.SteppingOver:
                    mustStop = FrameIsInStopList(currentFrame);
                    // по пути следования все равно может встретиться breakpoint
                    if (!mustStop && HitBreakpointOnLine(module, currentFrame))
                    {
                        _currentState = DebugState.Running; //для правильной причины останова (см. ниже)
                        mustStop = true;
                    }
                    break;
            }

            if (mustStop)
            {
                // здесь мы уже останавливались
                if (_lastStopPoint.frame != currentFrame || _lastStopPoint.line != currentFrame.LineNumber)
                {
                    if (_currentState == DebugState.Running)
                        LastStopReason = MachineStopReason.Breakpoint;
                    else
                        LastStopReason = MachineStopReason.Step;

                    _lastStopPoint = new StopPoint()
                    {
                        frame = currentFrame,
                        line = currentFrame.LineNumber
                    };
                    _currentState = DebugState.Running;
                }
                else
                {
                    mustStop = false;
                }
            }

            return mustStop;
            
        }
        
        private bool HitBreakpointOnLine(string module, ExecutionFrame currentFrame)
        {
            return _breakpoints.Find(module, currentFrame.LineNumber);
        }

        private bool FrameIsInStopList(ExecutionFrame currentFrame)
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

        public void StepIn()
        {
            _currentState = DebugState.SteppingIn;
        }

        internal void StepOut(ExecutionFrame currentFrame)
        {
            _currentState = DebugState.SteppingOut;
            _stopFrames = _machine.GetExecutionFrames().Select(x => x.FrameObject).Skip(1).ToArray();
        }

        internal void Continue()
        {
            _lastStopPoint = default(StopPoint);
        }
    }
}
