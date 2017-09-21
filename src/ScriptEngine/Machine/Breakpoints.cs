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
        private int _idsGenerator = 0;

        public int SetBreakpoint(string module, int line)
        {
            var bp = new Breakpoint(_idsGenerator++)
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

        public void Clear()
        {
            _breakpoints.Clear();
            _idsGenerator = 0;
        }

        public bool Find(string module, int line)
        {
            var found = _breakpoints.Find(x => x.Module.Equals(module) && x.LineNumber == line);
            return found != null;
        }
    }
}
