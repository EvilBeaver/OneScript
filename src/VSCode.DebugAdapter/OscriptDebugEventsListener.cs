/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Runtime.CompilerServices;
using EvilBeaver.DAP.Dto.Events;
using EvilBeaver.DAP.Server;
using OneScript.DebugProtocol;
using Serilog;

namespace VSCode.DebugAdapter
{
    public class OscriptDebugEventsListener : IDebugEventListener
    {
        private readonly IClientChannel _channel;
        private readonly ThreadStateContainer _threadState;
        private readonly ILogger Log = Serilog.Log.ForContext<OscriptDebugEventsListener>();

        public OscriptDebugEventsListener(IClientChannel channel, ThreadStateContainer threadState)
        {
            _channel = channel;
            _threadState = threadState;
        }

        public void ThreadStopped(int threadId, ThreadStopReason reason)
        {
            LogEventOccured();
            _threadState.Reset();
            _channel.SendEventAsync(new StoppedEvent
            {
                Body = new StoppedEventBody
                {
                    ThreadId = threadId,
                    Reason = reason.ToString(),
                    AllThreadsStopped = true
                }
            });
        }
        
        public void ThreadStoppedEx(int threadId, ThreadStopReason reason, string errorMessage)
        {
            LogEventOccured();
            _threadState.Reset();

            if (!string.IsNullOrEmpty(errorMessage))
                SendOutput("stderr", errorMessage);

            _channel.SendEventAsync(new StoppedEvent
            {
                Body = new StoppedEventBody
                {
                    ThreadId = threadId,
                    Reason = reason.ToString(),
                    AllThreadsStopped = true
                }
            });
        }
        
        public void ProcessExited(int exitCode)
        {
            LogEventOccured();
            _channel.SendEventAsync(new ExitedEvent
            {
                Body = new ExitedEventBody
                {
                    ExitCode = exitCode
                }
            });
        }
        
        private void SendOutput(string category, string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                if (data[data.Length - 1] != '\n')
                {
                    data += '\n';
                }
                _channel.SendEventAsync(new OutputEvent
                {
                    Body = new OutputEventBody
                    {
                        Category = category,
                        Output = data
                    }
                });
            }
        }
        
        private void LogEventOccured([CallerMemberName] string eventName = "")
        {
            Log.Debug("Event occured {Event}", eventName);
        }
    }
}
