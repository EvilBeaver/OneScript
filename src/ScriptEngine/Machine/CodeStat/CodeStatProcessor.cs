/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ScriptEngine.Machine
{
    /// <summary>
    /// Сборщик статистики исполнения кода. Его вызывают все потоки, где исполняется код,
    /// в том числе фоновые задания: коллекции потокобезопасные, а время каждый поток меряет сам.
    /// </summary>
    public class CodeStatProcessor : ICodeStatCollector
    {
        private readonly ConcurrentDictionary<CodeStatEntry, EntryStat> _stats =
            new ConcurrentDictionary<CodeStatEntry, EntryStat>();

        private readonly ConcurrentDictionary<string, bool> _preparedScripts =
            new ConcurrentDictionary<string, bool>();

        // Точка, которую сейчас исполняет поток: ей идет время до перехода к следующей точке
        private readonly ThreadLocal<ActiveEntry> _activeEntry =
            new ThreadLocal<ActiveEntry>(() => new ActiveEntry());

        // Делегат создается один раз: лямбда в вызове GetOrAdd выделяла бы память на каждую строку
        private readonly Func<CodeStatEntry, EntryStat> _newStat;

        // Отчет идет в порядке регистрации точек
        private long _registrationOrder;

        public CodeStatProcessor()
        {
            _newStat = _ => new EntryStat(Interlocked.Increment(ref _registrationOrder));
        }

        public bool IsPrepared(string ScriptFileName)
        {
            return _preparedScripts.ContainsKey(ScriptFileName);
        }

        public void MarkEntryReached(CodeStatEntry entry, int count = 1)
        {
            var stat = _stats.GetOrAdd(entry, _newStat);
            if (count == 0)
                return;

            Interlocked.Add(ref stat.Count, count);
            SwitchTo(stat);
        }

        public void MarkPrepared(string scriptFileName)
        {
            _preparedScripts.TryAdd(scriptFileName, true);
        }

        /// <summary>
        /// Снимок статистики. Можно брать, пока код еще исполняется в других потоках.
        /// </summary>
        public CodeStatDataCollection GetStatData()
        {
            var data = new CodeStatDataCollection();
            foreach (var item in _stats.ToArray().OrderBy(x => x.Value.Order))
            {
                if (!IsPrepared(item.Key.ScriptFileName))
                {
                    continue;
                }
                data.Add(new CodeStatData(item.Key, item.Value.ElapsedMilliseconds, Volatile.Read(ref item.Value.Count)));
            }

            return data;
        }

        /// <summary>
        /// Завершает замер времени в текущем потоке
        /// </summary>
        public void EndCodeStat()
        {
            StopCurrentWatch();
        }

        public void StopCurrentWatch()
        {
            SwitchTo(null);
        }

        public void StopWatch(CodeStatEntry entry)
        {
            if (_stats.TryGetValue(entry, out var stat) && _activeEntry.Value.Stat == stat)
            {
                SwitchTo(null);
            }
        }

        public void ResumeWatch(CodeStatEntry entry)
        {
            _stats.TryGetValue(entry, out var stat);
            SwitchTo(stat);
        }

        private void SwitchTo(EntryStat next)
        {
            var active = _activeEntry.Value;
            var now = Stopwatch.GetTimestamp();
            active.Stat?.AddElapsed(now - active.Since);
            active.Stat = next;
            active.Since = now;
        }

        private sealed class EntryStat
        {
            public readonly long Order;
            public int Count;
            private long _elapsedTicks;

            public EntryStat(long order)
            {
                Order = order;
            }

            public void AddElapsed(long ticks) => Interlocked.Add(ref _elapsedTicks, ticks);

            // Так же, как Stopwatch.ElapsedMilliseconds
            public long ElapsedMilliseconds =>
                Stopwatch.GetElapsedTime(0, Interlocked.Read(ref _elapsedTicks)).Ticks / TimeSpan.TicksPerMillisecond;
        }

        private sealed class ActiveEntry
        {
            public EntryStat Stat;
            public long Since;
        }
    }
}
