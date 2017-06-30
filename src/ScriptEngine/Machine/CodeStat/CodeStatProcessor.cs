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

        public void MarkPrepared(string ScriptFileName)
        {
            _preparedScripts.Add(ScriptFileName);
        }

        public CodeStatDataCollection GetStatData()
        {
            CodeStatDataCollection data = new CodeStatDataCollection();
            foreach (var item in _codeStat)
            {
                data.Add(new CodeStatData(item.Key, _watchers[item.Key].ElapsedMilliseconds, item.Value));
            }
            
            return data;
            //var w = new StreamWriter(_outputFileName);
            //var jwriter = new JsonTextWriter(w);
            //jwriter.Formatting = Formatting.Indented;

            //jwriter.WriteStartObject();
            //foreach (var source in _codeStat.GroupBy((arg) => arg.Key.ScriptFileName))
            //{
            //    jwriter.WritePropertyName(source.Key, true);
            //    jwriter.WriteStartObject();

            //    jwriter.WritePropertyName("#path");
            //    jwriter.WriteValue(source.Key);
            //    foreach (var method in source.GroupBy((arg) => arg.Key.SubName))
            //    {
            //        jwriter.WritePropertyName(method.Key, true);
            //        jwriter.WriteStartObject();

            //        foreach (var entry in method.OrderBy((kv) => kv.Key.LineNumber))
            //        {
            //            jwriter.WritePropertyName(entry.Key.LineNumber.ToString());
            //            jwriter.WriteStartObject();

            //            jwriter.WritePropertyName("count");
            //            jwriter.WriteValue(entry.Value);

            //            if (_watchers.ContainsKey(entry.Key))
            //            {
            //                var elapsed = _watchers[entry.Key].ElapsedMilliseconds;

            //                jwriter.WritePropertyName("time");
            //                jwriter.WriteValue(elapsed);
            //            }

            //            jwriter.WriteEndObject();
            //        }

            //        jwriter.WriteEndObject();
            //    }
            //    jwriter.WriteEndObject();
            //}
            //jwriter.WriteEndObject();
            //jwriter.Flush();

            //_codeStat.Clear();
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
