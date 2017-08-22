using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
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

    class Breakpoints
    {
        private readonly List<Breakpoint> _breakpoints = new List<Breakpoint>();
        private int idsGenerator = 0;

        public int SetBreakpoint(string module, int line)
        {
            var bp = new Breakpoint(idsGenerator++)
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
            if (index >= 0)
                _breakpoints.RemoveAt(index);
        }

        public bool Find(string module, int line)
        {
            var found = _breakpoints.Find(x => x.Module.Equals(module) && x.LineNumber == line);
            return found != null;
        }
    }
}
