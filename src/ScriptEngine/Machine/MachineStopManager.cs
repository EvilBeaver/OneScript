using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
   
    enum StopKind
    {
        SourceLine,
        MethodEntry,
        MethodReturn,
        NextLine
    }

    struct StopHandle
    {
        public StopKind kind;
        public string source;
        public int line;
        public ExecutionFrame frame;
    }

    class MachineStopManager
    {
        List<StopHandle> _registeredStops = new List<StopHandle>();
        Stack<ExecutionFrame> _callStack = new Stack<ExecutionFrame>();

        private bool _mustStopOnMethodEntry = false;

        internal void AddSourceLineStop(string source, int line)
        {
            _registeredStops.Add(new StopHandle()
            {
                kind = StopKind.SourceLine,
                line = line,
                source = source
            });
        }

        internal void AddStopAtMethodEntry()
        {
            _mustStopOnMethodEntry = true;
        }

        internal void AddNextLineStop(ExecutionFrame currentFrame)
        {
            _registeredStops.Add(new StopHandle()
            {
                kind = StopKind.NextLine,
                frame = currentFrame
            });
        }

        internal void AddStopOnMethodExit()
        {
            _registeredStops.Add(new StopHandle()
            {
                kind = StopKind.MethodReturn,
                frame = _callStack.Peek()
            });
        }

        internal void OnFrameEntered(ExecutionFrame frame)
        {
            _callStack.Push(frame);
            if (_mustStopOnMethodEntry)
            {
                _registeredStops.Add(new StopHandle()
                {
                    frame = frame,
                    kind = StopKind.MethodEntry
                });
                _mustStopOnMethodEntry = false;
            }
        }

        internal void OnFrameExited(out bool shouldStop)
        {
            var frame = _callStack.Pop();
            var itemIdx = _registeredStops.FindIndex(x => x.kind == StopKind.MethodReturn && x.frame == frame);
            if (itemIdx >= 0)
            {
                shouldStop = true;
                _registeredStops.RemoveAt(itemIdx);
            }
            else
            {
                shouldStop = false;
            }
        }
        
        internal bool ShouldStopAtThisLine(string module, ExecutionFrame frame)
        {
            for (int i = _registeredStops.Count-1; i >=0; i--)
            {
                var stop = _registeredStops[i];
                if (stop.kind == StopKind.SourceLine && stop.source == module && stop.line == frame.LineNumber)
                {
                    return true;
                }

                if ((stop.kind == StopKind.NextLine||stop.kind == StopKind.MethodEntry) && stop.frame == frame)
                {
                    _registeredStops.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void ClearSteppingStops()
        {
            var copy = _registeredStops.Where(x => x.kind == StopKind.SourceLine).ToList();
            _registeredStops = copy;
        }
    }
}
