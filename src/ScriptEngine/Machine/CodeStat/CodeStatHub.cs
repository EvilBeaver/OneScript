/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace ScriptEngine.Machine
{
    public sealed class CodeStatHub : ICodeStatCollector
    {
        private readonly object _lock = new object();
        private readonly List<CodeStatEntry> _knownEntries = new List<CodeStatEntry>();
        private readonly HashSet<CodeStatEntry> _knownSet = new HashSet<CodeStatEntry>();
        private readonly HashSet<string> _preparedScripts = new HashSet<string>();

        private CodeStatProcessor[] _alive = Array.Empty<CodeStatProcessor>();
        private CodeStatProcessor[] _active = Array.Empty<CodeStatProcessor>();

        public CodeStatProcessor StartSession()
        {
            var session = new CodeStatProcessor(this);
            lock (_lock)
            {
                _alive = Append(_alive, session);
                _active = Append(_active, session);
            }

            return session;
        }

        public void PauseSession(CodeStatProcessor session)
        {
            lock (_lock)
            {
                session.StopActiveWatch();
                _active = Remove(_active, session);
            }
        }

        public void ResumeSession(CodeStatProcessor session)
        {
            lock (_lock)
            {
                if (Array.IndexOf(_alive, session) < 0)
                    return;
                if (Array.IndexOf(_active, session) >= 0)
                    return;
                _active = Append(_active, session);
            }
        }

        public void FinishSession(CodeStatProcessor session)
        {
            lock (_lock)
            {
                session.EndCodeStat();
                session.FreezeCatalog(_knownEntries.ToArray(), new HashSet<string>(_preparedScripts));
                _active = Remove(_active, session);
                _alive = Remove(_alive, session);
            }
        }

        internal CodeStatDataCollection GetLiveStatData(CodeStatProcessor session)
        {
            lock (_lock)
            {
                return session.BuildFromCatalog(_knownEntries, _knownEntries.Count, _preparedScripts);
            }
        }

        public bool IsPrepared(string ScriptFileName)
        {
            lock (_lock)
            {
                return _preparedScripts.Contains(ScriptFileName);
            }
        }

        public void MarkEntryReached(CodeStatEntry entry, int count = 1)
        {
            lock (_lock)
            {
                if (_knownSet.Add(entry))
                    _knownEntries.Add(entry);

                if (count == 0)
                    return;

                foreach (var session in _active)
                    session.MarkEntryReached(entry, count);
            }
        }

        public void MarkPrepared(string scriptFileName)
        {
            lock (_lock)
            {
                _preparedScripts.Add(scriptFileName);
            }
        }

        public void StopWatch(CodeStatEntry entry)
        {
            lock (_lock)
            {
                foreach (var session in _active)
                    session.StopWatch(entry);
            }
        }

        public void ResumeWatch(CodeStatEntry entry)
        {
            lock (_lock)
            {
                foreach (var session in _active)
                    session.ResumeWatch(entry);
            }
        }

        private static T[] Append<T>(T[] source, T item)
        {
            var result = new T[source.Length + 1];
            Array.Copy(source, result, source.Length);
            result[source.Length] = item;
            return result;
        }

        private static T[] Remove<T>(T[] source, T item) where T : class
        {
            var index = Array.IndexOf(source, item);
            if (index < 0)
                return source;
            if (source.Length == 1)
                return Array.Empty<T>();

            var result = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, result, 0, index);
            if (index < source.Length - 1)
                Array.Copy(source, index + 1, result, index, source.Length - index - 1);
            return result;
        }
    }
}
