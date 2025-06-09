/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    public class ThreadManager : IThreadManager
    {
        private readonly IDictionary<int, MachineWaitToken> _machinesOnThreads = new ConcurrentDictionary<int, MachineWaitToken>();

        public MachineWaitToken GetTokenForThread(int threadId)
        {
            if (_machinesOnThreads.TryGetValue(threadId, out var value))
            {
                return value;
            }

            throw new ArgumentOutOfRangeException($"Thread {threadId} is unregistered");
        }
        
        public event EventHandler<ThreadStoppedEventArgs> ThreadStopped;
        
        public MachineWaitToken[] GetAllTokens()
        {
            return _machinesOnThreads.Values.ToArray();
        }

        private void EmitThreadStopped(int threadId, MachineStopReason reason, string errMessage)
        {
            var machine = GetTokenForThread(threadId).Machine;
            
            var args = new ThreadStoppedEventArgs
            {
                Machine = machine,
                ThreadId = threadId,
                StopReason = reason,
                ErrorMessage = errMessage
            };
            
            ThreadStopped?.Invoke(this, args);
        }

        public void ReleaseAllThreads()
        {
            var tokens = GetAllTokens();
            foreach (var machineWaitToken in tokens)
            {
                machineWaitToken.Machine.UnsetDebugMode();
                machineWaitToken.Dispose();
            }

            _machinesOnThreads.Clear();
        }
        
        public void Detach(int threadId)
        {
            if (_machinesOnThreads.Remove(threadId, out var t))
            {
                // Если машина была остановлена - продолжаем её уже без остановок
                t.Machine.UnsetDebugMode();
                t.Set();
            }
        }
        
        public void Dispose()
        {
            ReleaseAllThreads();
        }

        public void ThreadStarted(int threadId, MachineInstance machine)
        {
            _machinesOnThreads[threadId] = new MachineWaitToken
            {
                Machine = machine
            };
        }

        void IThreadManager.ThreadStopped(int threadId, MachineStopReason reason, string errorMessage)
        {
            EmitThreadStopped(threadId, reason, errorMessage);
        }

        public void ThreadExited(int threadId)
        {
            _machinesOnThreads.Remove(threadId);
        }

        public IEnumerable<int> GetThreadIds()
        {
            return _machinesOnThreads.Keys.ToList();
        }
    }
}