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
        private readonly List<BreakpointDescriptor> _breakpoints = new List<BreakpointDescriptor>();
        private int _idsGenerator;

        public void SetLineStops(string module, int[] lines)
        {
            var cleaned = _breakpoints.Where(x => x.Module != module)
                .ToList();

            var range = lines.Select(x => new BreakpointDescriptor(_idsGenerator++) { LineNumber = x, Module = module });
            cleaned.AddRange(range);
            _breakpoints.Clear();
            _breakpoints.AddRange(cleaned);
        }

        public bool Find(string module, int line)
        {
            var found = _breakpoints.Find(x => x.Module.Equals(module) && x.LineNumber == line);
            return found != null;
        }
    }
}