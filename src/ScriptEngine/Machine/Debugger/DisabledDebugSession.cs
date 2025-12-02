/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Machine.Debugger
{
    public sealed class DisabledDebugSession : IDebugSession
    {
        public IBreakpointManager BreakpointManager { get; } = new NoOpBreakpointManager();
        public IThreadEventsListener ThreadManager { get; } = new NoOpThreadEventsListener();
        public bool IsActive => false;
        
        public void WaitReadyToRun()
        {
        }
        public void Dispose()
        {
        }

        private class NoOpBreakpointManager : IBreakpointManager
        {
            public void SetExceptionBreakpoints((string Id, string Condition)[] filters)
            {
            }

            public void SetBreakpoints(string module, (int Line, string Condition)[] breakpoints)
            {
            }

            public bool StopOnAnyException(string message) => false;

            public bool StopOnUncaughtException(string message) => false;

            public bool FindBreakpoint(string module, int line) => false;

            public string GetCondition(string module, int line) => "";

            public void Clear()
            {
            }
        }

        private class NoOpThreadEventsListener : IThreadEventsListener
        {
            public void ThreadStarted(int threadId, MachineInstance machine)
            {
            }

            public void ThreadStopped(int threadId, MachineStopReason reason, string errorMessage)
            {
            }

            public void ThreadExited(int threadId)
            {
            }
        }
    }
}