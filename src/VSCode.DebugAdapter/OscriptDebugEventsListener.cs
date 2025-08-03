/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Runtime.CompilerServices;
using OneScript.DebugProtocol;
using Serilog;
using VSCodeDebug;

namespace VSCode.DebugAdapter
{
    public class OscriptDebugEventsListener : IDebugEventListener
    {
        private readonly DebugSession _session;
        private readonly ThreadStateContainer _threadState;
        private readonly ILogger Log = Serilog.Log.ForContext<OscriptDebugEventsListener>();

        public OscriptDebugEventsListener(DebugSession session, ThreadStateContainer threadState)
        {
            _session = session;
            _threadState = threadState;
        }

        public void ThreadStopped(int threadId, ThreadStopReason reason)
        {
            LogEventOccured();
            _threadState.Reset();
            _session.SendEvent(new StoppedEvent(threadId, reason.ToString()));
        }
        
        public void ThreadStoppedEx(int threadId, ThreadStopReason reason, string errorMessage)
        {
            LogEventOccured();
            _threadState.Reset();

            if (!string.IsNullOrEmpty(errorMessage))
                SendOutput("stderr", errorMessage);

            _session.SendEvent(new StoppedEvent(threadId, reason.ToString()));
        }
        
        public void ProcessExited(int exitCode)
        {
            LogEventOccured();
            _session.SendEvent(new ExitedEvent(exitCode));
        }
        
        private void SendOutput(string category, string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                if (data[data.Length - 1] != '\n')
                {
                    data += '\n';
                }
                _session.SendEvent(new OutputEvent(category, data));
            }
        }
        
        private void LogEventOccured([CallerMemberName] string eventName = "")
        {
            Log.Debug("Event occured {Event}", eventName);
        }
    }
}
