/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using OneScript.DebugProtocol;
using ScriptEngine;
using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    internal class WcfDebugController : DebugControllerBase
    {
        private readonly int _port;
        private ServiceHost _serviceHost;
        private IDebugEventListener _eventChannel;

        public WcfDebugController(int listenerPort)
        {
            _port = listenerPort;
        }

        public ManualResetEventSlim ThreadEvent => DebugCommandEvent;

        private ThreadStopReason ConvertStopReason(MachineStopReason reason)
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

        public override void WaitForDebugEvent(DebugEventType theEvent)
        {
            switch (theEvent)
            {
                case DebugEventType.BeginExecution:

                    var host = new ServiceHost(this);
                    var binding = Binder.GetBinding();
                    host.AddServiceEndpoint(typeof(IDebuggerService), binding, Binder.GetDebuggerUri(_port));
                    _serviceHost = host;
                    host.Open();


                    DebugFsm.Start();
                    DebugCommandEvent.Wait(); // процесс 1скрипт не стартует, пока не получено разрешение от дебагера

                    break;
                case DebugEventType.Continue:
                    DebugCommandEvent.Reset();
                    DebugCommandEvent.Wait();
                    break;
                default:
                    throw new InvalidOperationException($"event {theEvent} cant't be waited");
            }

        }

        public override void NotifyProcessExit(int exitCode)
        {
            if (!CallbackChannelIsReady())
                return; // нет подписчика

            _eventChannel.ProcessExited(exitCode);
            _serviceHost?.Close();
        }
        
        protected override void OnMachineStopped(MachineInstance machine, MachineStopReason reason)
        {
            if (!CallbackChannelIsReady())
                return; // нет подписчика

            DebugCommandEvent.Reset();
            _eventChannel.ThreadStopped(1, ConvertStopReason(reason));
            DebugCommandEvent.Wait();
        }

        private bool CallbackChannelIsReady()
        {
            if (_eventChannel != null)
            {
                var ico = (ICommunicationObject)_eventChannel;
                return ico.State == CommunicationState.Opened;
            }
            return false;
        }

        public void SetCallback(IDebugEventListener eventChannel)
        {
            _eventChannel = eventChannel;
        }
    }
}
