/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    public class DefaultBreakpointManager : IBreakpointManager
    {
        private readonly List<string> _exceptionBreakpointsFilters = new List<string>();
        private readonly List<BreakpointDescriptor> _breakpoints = new List<BreakpointDescriptor>();
        private int _idsGenerator;

        public void SetExceptionBreakpoints(string[] filters)
        {
            _exceptionBreakpointsFilters.Clear();
            _exceptionBreakpointsFilters.AddRange(filters);
        }

        public void SetBreakpoints(string module, (int Line, string Condition)[] breakpoints)
        {
            var cleaned = _breakpoints.Where(x => x.Module != module)
                .ToList();

            var range = breakpoints.Select(x => new BreakpointDescriptor(_idsGenerator++) { LineNumber = x.Line, Module = module, Condition = x.Condition });
            cleaned.AddRange(range);
            _breakpoints.Clear();
            _breakpoints.AddRange(cleaned);
        }

        public bool FindBreakpoint(string module, int line)
        {
            return _breakpoints.Find(x => x.Module.Equals(module) && x.LineNumber == line) != null;
        }

        public string GetCondition(string module, int line)
            => _breakpoints.Find(x => x.Module.Equals(module) && x.LineNumber == line).Condition;

        public void Clear()
        {
            _breakpoints.Clear();
        }

        public bool StopInAnyException()
            => _exceptionBreakpointsFilters.Contains("all");

        public bool StopInUncaughtException()
            => _exceptionBreakpointsFilters.Contains("uncaught");
    }
}