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
using OneScript.DebugProtocol;
using OneScript.DebugServices;
using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    [Obsolete]
    internal class MachineWaitToken
    {
        public MachineInstance Machine;

        public ManualResetEventSlim ThreadEvent;
    }


    [Obsolete]
    internal abstract class DebugControllerBase : IDebugController
    {
        private readonly Dictionary<int, MachineWaitToken> _machinesOnThreads = new Dictionary<int, MachineWaitToken>();

        public DebugControllerBase()
        {
            BreakpointManager = new DefaultBreakpointManager();
        }
        
        public virtual void Init()
        {

        }

        public virtual void Wait()
        {
            var token = GetTokenForThread(Thread.CurrentThread.ManagedThreadId);
            token.ThreadEvent = new ManualResetEventSlim();
            token.ThreadEvent.Wait();
        }

        public virtual void NotifyProcessExit(int exitCode)
        {
            var t = _machinesOnThreads[Thread.CurrentThread.ManagedThreadId];
            t.Machine.MachineStopped -= Machine_MachineStopped;
            _machinesOnThreads.Remove(Thread.CurrentThread.ManagedThreadId);
        }

        public void DetachFromThread()
        {
            if (_machinesOnThreads.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var t))
            {
                t.Machine.MachineStopped -= Machine_MachineStopped;
                _machinesOnThreads.Remove(Thread.CurrentThread.ManagedThreadId);
            }
        }

        public IBreakpointManager BreakpointManager { get; set; }

        public virtual void AttachToThread()
        {
            var machine = MachineInstance.Current;
            _machinesOnThreads[Thread.CurrentThread.ManagedThreadId] = new MachineWaitToken()
            {
                Machine = machine
            };

            machine.MachineStopped += Machine_MachineStopped;
        }

        private void Machine_MachineStopped(object sender, MachineStoppedEventArgs e)
        {
            OnMachineStopped((MachineInstance)sender, e.Reason);
        }

        protected abstract void OnMachineStopped(MachineInstance machine, MachineStopReason reason);

        public void Dispose()
        {
            Dispose(true);
        }

        public MachineWaitToken GetTokenForThread(int threadId)
        {
            return _machinesOnThreads[threadId];
        }

        public MachineWaitToken[] GetAllTokens()
        {
            return _machinesOnThreads.Values.ToArray();
        }

        public int[] GetAllThreadIds()
        {
            return _machinesOnThreads.Keys.ToArray();
        }

        protected virtual void Dispose(bool disposing)
        {
            var tokens = GetAllTokens();
            foreach (var machineWaitToken in tokens)
            {
                machineWaitToken.Machine.MachineStopped -= Machine_MachineStopped;
            }

            _machinesOnThreads.Clear();
        }

        protected ThreadStopReason ConvertStopReason(MachineStopReason reason)
        {
            switch(reason)
            {
                case MachineStopReason.Breakpoint:
                    return ThreadStopReason.Breakpoint;
                case MachineStopReason.Step:
                    return ThreadStopReason.Step;
                case MachineStopReason.Exception:
                    return ThreadStopReason.Exception;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
