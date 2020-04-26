/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    public class ThreadManager : IDisposable
    {
        private readonly Dictionary<int, MachineWaitToken> _machinesOnThreads = new Dictionary<int, MachineWaitToken>();

        public MachineWaitToken GetTokenForThread(int threadId)
        {
            return _machinesOnThreads[threadId];
        }
        
        public MachineWaitToken GetTokenForCurrentThread()
        {
            return GetTokenForThread(Thread.CurrentThread.ManagedThreadId);
        }

        public event EventHandler<ThreadStoppedEventArgs> ThreadStopped;

        public void AttachToCurrentThread()
        {
            var machine = MachineInstance.Current;
            _machinesOnThreads[Thread.CurrentThread.ManagedThreadId] = new MachineWaitToken()
            {
                Machine = machine
            };

            machine.MachineStopped += Machine_MachineStopped;
        }
        
        public void DetachFromCurrentThread()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            DetachFromThread(threadId);
        }
        
        public void DetachFromThread(int threadId)
        {
            if (_machinesOnThreads.TryGetValue(threadId, out var t))
            {
                t.Machine.MachineStopped -= Machine_MachineStopped;
                _machinesOnThreads.Remove(threadId);
            }
        }
        
        public MachineWaitToken[] GetAllTokens()
        {
            return _machinesOnThreads.Values.ToArray();
        }

        private void Machine_MachineStopped(object sender, MachineStoppedEventArgs e)
        {
            var args = new ThreadStoppedEventArgs
            {
                Machine = (MachineInstance)sender,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                StopReason = e.Reason
            };
            
            ThreadStopped?.Invoke(this, args);
        }
        
        public int[] GetAllThreadIds()
        {
            return _machinesOnThreads.Keys.ToArray();
        }
        
        public void Dispose()
        {
            var tokens = GetAllTokens();
            foreach (var machineWaitToken in tokens)
            {
                machineWaitToken.Machine.MachineStopped -= Machine_MachineStopped;
            }

            _machinesOnThreads.Clear();
        }
    }
}