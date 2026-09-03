/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Diagnostics;

namespace ScriptEngine.Machine
{
    public class CodeStatProcessor : ICodeStatCollector
    {
        private readonly CodeStatHub _hub;
        private readonly Dictionary<CodeStatEntry, int> _codeStat = new Dictionary<CodeStatEntry, int>();
        private readonly Dictionary<CodeStatEntry, Stopwatch> _watchers = new Dictionary<CodeStatEntry, Stopwatch>();
        private readonly HashSet<string> _preparedScripts = new HashSet<string>();

        private Stopwatch _activeStopwatch;
        private IReadOnlyList<CodeStatEntry> _frozenEntries;
        private int _frozenCount;
        private HashSet<string> _frozenPrepared;

        public CodeStatProcessor()
        {
        }

        internal CodeStatProcessor(CodeStatHub hub)
        {
            _hub = hub;
        }

        public bool IsPrepared(string ScriptFileName)
        {
            return _preparedScripts.Contains(ScriptFileName);
        }

        public void MarkEntryReached(CodeStatEntry entry, int count = 1)
        {
            _codeStat.TryGetValue(entry, out var oldValue);
            _codeStat[entry] = oldValue + count;

            if (count == 0)
                return;

            _activeStopwatch?.Stop();
            if (!_watchers.TryGetValue(entry, out var watch))
            {
                watch = new Stopwatch();
                _watchers[entry] = watch;
            }

            _activeStopwatch = watch;
            _activeStopwatch.Start();
        }

        public void MarkPrepared(string scriptFileName)
        {
            _preparedScripts.Add(scriptFileName);
        }

        public CodeStatDataCollection GetStatData()
        {
            if (_frozenEntries != null)
                return BuildFromCatalog(_frozenEntries, _frozenCount, _frozenPrepared);

            if (_hub != null)
                return _hub.GetLiveStatData(this);

            var data = new CodeStatDataCollection();
            foreach (var item in _codeStat)
            {
                if (!IsPrepared(item.Key.ScriptFileName))
                    continue;

                long time = 0;
                if (_watchers.TryGetValue(item.Key, out var watch))
                    time = watch.ElapsedMilliseconds;
                data.Add(new CodeStatData(item.Key, time, item.Value));
            }

            return data;
        }

        internal void FreezeCatalog(CodeStatEntry[] entries, HashSet<string> prepared)
        {
            _frozenEntries = entries;
            _frozenCount = entries.Length;
            _frozenPrepared = prepared;
        }

        internal CodeStatDataCollection BuildFromCatalog(
            IReadOnlyList<CodeStatEntry> entries,
            int count,
            HashSet<string> prepared)
        {
            var data = new CodeStatDataCollection();
            for (var i = 0; i < count; i++)
            {
                var entry = entries[i];
                if (!prepared.Contains(entry.ScriptFileName))
                    continue;

                _codeStat.TryGetValue(entry, out var executionCount);
                long time = 0;
                if (_watchers.TryGetValue(entry, out var watch))
                    time = watch.ElapsedMilliseconds;

                data.Add(new CodeStatData(entry, time, executionCount));
            }

            return data;
        }

        public void EndCodeStat()
        {
            StopActiveWatch();
        }

        public void StopActiveWatch()
        {
            _activeStopwatch?.Stop();
            _activeStopwatch = null;
        }

        public void StopWatch(CodeStatEntry entry)
        {
            if (_watchers.TryGetValue(entry, out var watch))
                watch.Stop();
        }

        public void ResumeWatch(CodeStatEntry entry)
        {
            _activeStopwatch?.Stop();

            if (_watchers.TryGetValue(entry, out var watch))
            {
                _activeStopwatch = watch;
                _activeStopwatch.Start();
            }
        }
    }
}
