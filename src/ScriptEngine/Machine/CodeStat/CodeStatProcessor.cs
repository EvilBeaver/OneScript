/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace ScriptEngine.Machine
{
    public class CodeStatProcessor : ICodeStatCollector
    {
        private Dictionary<CodeStatEntry, int> _codeStat = new Dictionary<CodeStatEntry, int>();
        private Dictionary<CodeStatEntry, Stopwatch> _watchers = new Dictionary<CodeStatEntry, Stopwatch>();
        private Stopwatch _activeStopwatch = null;
        private HashSet<string> _preparedScripts = new HashSet<string>();

        public CodeStatProcessor()
        {
        }

        public bool IsPrepared(string ScriptFileName)
        {
            return _preparedScripts.Contains(ScriptFileName);
        }

        public void MarkEntryReached(CodeStatEntry entry, int count = 1)
        {
            int oldValue = 0;
            _codeStat.TryGetValue(entry, out oldValue);
            _codeStat[entry] = oldValue + count;

            if (count == 0)
            {
                if (!_watchers.ContainsKey(entry))
                {
                    _watchers.Add(entry, new Stopwatch());
                }
            }
            else
            {
                _activeStopwatch?.Stop();
                _activeStopwatch = _watchers[entry];
                _activeStopwatch.Start();
            }
        }

        public void MarkPrepared(string scriptFileName)
        {
            _preparedScripts.Add(scriptFileName);
        }

        public CodeStatDataCollection GetStatData()
        {
            CodeStatDataCollection data = new CodeStatDataCollection();
            foreach (var item in _codeStat)
            {
                if (!IsPrepared(item.Key.ScriptFileName))
                {
                    continue;
                }
                data.Add(new CodeStatData(item.Key, _watchers[item.Key].ElapsedMilliseconds, item.Value));
            }
            
            return data;
        }

        public void EndCodeStat()
        {
            _activeStopwatch?.Stop();
        }

        public void StopWatch(CodeStatEntry entry)
        {
            if (_watchers.ContainsKey(entry))
            {
                _watchers[entry].Stop();
            }
        }

        public void ResumeWatch(CodeStatEntry entry)
        {
            _activeStopwatch?.Stop();

            if (_watchers.ContainsKey(entry))
            {
                _activeStopwatch = _watchers[entry];
                _activeStopwatch.Start();
            }
        }
    }
}
