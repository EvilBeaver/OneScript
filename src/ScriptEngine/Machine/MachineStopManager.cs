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
        public ExecutionFrame[] frames;
    }

    class MachineStopManager
    {
        List<StopHandle> _registeredStops = new List<StopHandle>();

        internal void AddSourceLineStop(string source, int line)
        {
            _registeredStops.Add(new StopHandle()
            {
                kind = StopKind.SourceLine,
                line = line,
                source = source
            });
        }

        internal void AddNextLineStop(ExecutionFrame currentFrame)
        {
            _registeredStops.Add(new StopHandle()
            {
                kind = StopKind.NextLine,
                frames = new []{ currentFrame }
            });
        }

        internal void StopOnMethodEntry(ExecutionFrame returnFrame)
        {
            _registeredStops.Add(new StopHandle()
            {
                kind = StopKind.MethodEntry,
                frames = new ExecutionFrame[2]
                {
                    null,
                    returnFrame
                }
            });
        }
        
        internal void OnFrameEntered(ExecutionFrame frame)
        {
            int idx = _registeredStops.FindIndex(x => x.kind == StopKind.MethodEntry);
            if (idx >= 0)
            {
                _registeredStops[idx].frames[0] = frame;
            }
        }

        internal void OnFrameExited(ExecutionFrame frame)
        {

        }

        internal bool ShouldStopHere(string module, ExecutionFrame frame)
        {
            for (int i = _registeredStops.Count-1; i >=0; i--)
            {
                var stop = _registeredStops[i];
                if (stop.kind == StopKind.SourceLine && stop.source == module && stop.line == frame.LineNumber)
                {
                    return true;
                }

                if ((stop.kind == StopKind.NextLine||stop.kind == StopKind.MethodEntry) && stop.frames.Contains(frame))
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
