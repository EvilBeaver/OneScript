/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.ServiceModel;
using System.Threading;
using OneScript.DebugProtocol;
using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    [Obsolete]
    internal class WcfDebugController : DebugControllerBase
    {
        private readonly int _port;
        private ServiceHost _serviceHost;
        private IDebugEventListener _eventChannel;

        public WcfDebugController(int listenerPort)
        {
            _port = listenerPort;
        }

        public override void Init()
        {
            var serviceInstance = new WcfDebugService(this);
            var host = new ServiceHost(serviceInstance);
            var binding = (NetTcpBinding)Binder.GetBinding();
            binding.MaxBufferPoolSize = DebuggerSettings.MAX_BUFFER_SIZE;
            binding.MaxBufferSize = DebuggerSettings.MAX_BUFFER_SIZE;
            binding.MaxReceivedMessageSize = DebuggerSettings.MAX_BUFFER_SIZE;
            host.AddServiceEndpoint(typeof(IDebuggerService), binding, Binder.GetDebuggerUri(_port));
            _serviceHost = host;
            host.Open();

        }

        public override void NotifyProcessExit(int exitCode)
        {
            base.NotifyProcessExit(exitCode);
            if (!CallbackChannelIsReady())
                return; // нет подписчика

            _eventChannel.ProcessExited(exitCode);
            _serviceHost?.Close();
        }

        protected override void OnMachineStopped(MachineInstance machine, MachineStopReason reason)
        {
            if (!CallbackChannelIsReady())
                return; // нет подписчика

            var handle = GetTokenForThread(Thread.CurrentThread.ManagedThreadId);
            handle.ThreadEvent.Reset();
            _eventChannel.ThreadStopped(1, ConvertStopReason(reason));
            handle.ThreadEvent.Wait();
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
