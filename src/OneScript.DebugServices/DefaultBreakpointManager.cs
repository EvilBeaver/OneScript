/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Commons;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    public class DefaultBreakpointManager : IBreakpointManager
    {
        // Точки задает поток отладчика, а проверяют потоки скриптов: списки не меняются, а подменяются целиком
        private volatile Dictionary<string, string> _exceptionBreakpointsFilters = new Dictionary<string, string>();
        private volatile BreakpointDescriptor[] _breakpoints = Array.Empty<BreakpointDescriptor>();
        private readonly object _lock = new object();
        private int _idsGenerator;

        public void SetExceptionBreakpoints((string Id, string Condition)[] filters)
        {
            var newFilters = new Dictionary<string, string>();
            filters?.ForEach(c => newFilters.Add(c.Id, c.Condition));
            _exceptionBreakpointsFilters = newFilters;
        }

        public void SetBreakpoints(string module, (int Line, string Condition)[] breakpoints)
        {
            lock (_lock)
            {
                var cleaned = _breakpoints.Where(x => x.Module != module)
                    .ToList();

                var range = breakpoints.Select(x => new BreakpointDescriptor(_idsGenerator++) { LineNumber = x.Line, Module = module, Condition = x.Condition });
                cleaned.AddRange(range);
                _breakpoints = cleaned.ToArray();
            }
        }

        public bool FindBreakpoint(string module, int line)
            => Find(module, line) != null;

        // Точку могли снять между FindBreakpoint и GetCondition
        public string GetCondition(string module, int line)
            => Find(module, line)?.Condition;

        private BreakpointDescriptor Find(string module, int line)
            => Array.Find(_breakpoints, x => x.Module.Equals(module) && x.LineNumber == line);

        public void Clear()
        {
            lock (_lock)
            {
                _breakpoints = Array.Empty<BreakpointDescriptor>();
            }
            _exceptionBreakpointsFilters = new Dictionary<string, string>();
        }

        public bool StopOnAnyException(string message)
            => NeedStopOnException("all", message);

        public bool StopOnUncaughtException(string message)
            => NeedStopOnException("uncaught", message);

        private bool NeedStopOnException(string filterId, string message)
        {
            if (_exceptionBreakpointsFilters.TryGetValue(filterId, out var condition))
            {
                if (string.IsNullOrEmpty(condition))
                    return true;
                else
                    return message.Contains(condition);
            }

            return false;
        }
    }
}
