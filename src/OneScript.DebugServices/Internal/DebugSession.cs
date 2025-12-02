/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Threading;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Debugger;

namespace OneScript.DebugServices.Internal
{
    internal class DebugSession : IDebugSession
    {
        private bool _isStarted;
        private readonly ThreadManager _threadManager;
        private readonly TcpEventCallbackChannel _callbackChannel;
        private readonly DispatchingService<IDebuggerService> _messageServer;
        
        private readonly ManualResetEventSlim _startEvent = new ManualResetEventSlim();
        
        public bool IsActive { get; private set; }

        public DebugSession(IDebuggerClient connectedClient, bool attachMode)
        {
            // Если это режим Attach, то не ждем готовности к запуску
            // т.к. IDE не пришлет команду Execute и мы сразу считаем себя готовой сессией.
            _isStarted = attachMode;
            
            var channel = new JsonDtoChannel(connectedClient);
            var ipcServer = new DefaultMessageServer<RpcCall>(channel)
            {
                ServerThreadName = "debug-server"
            };

            ipcServer.OnError += CommunicationError;
            
            BreakpointManager = new DefaultBreakpointManager();
            _threadManager = new ThreadManager();
            _callbackChannel = new TcpEventCallbackChannel(channel);
            
            var commandsHandler = new DebuggerServiceImpl(this, new DefaultVariableVisualizer());
            
            _messageServer = new DispatchingService<IDebuggerService>(ipcServer, commandsHandler);
            _messageServer.Start();
            
            _threadManager.ThreadStopped += ThreadManagerOnThreadStopped;
            IsActive = true;
        }

        private void CommunicationError(object sender, CommunicationEventArgs e)
        {
            // Unexpected error in message loop
            Dispose();
        }

        private void ThreadManagerOnThreadStopped(object sender, ThreadStoppedEventArgs e)
        {
            MachineWaitToken token;
            try
            {
                token = _threadManager.GetTokenForThread(e.ThreadId);
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            
            token.Reset();
            
            _callbackChannel.ThreadStoppedEx(e.ThreadId, ConvertStopReason(e.StopReason), e.ErrorMessage);
            token.Wait();
        }

        public void Dispose()
        {
            _threadManager.ThreadStopped -= ThreadManagerOnThreadStopped;
            _threadManager.Dispose();
            _messageServer.Stop();
            IsActive = false;
            
            OnClose?.Invoke(this);
        }

        public IBreakpointManager BreakpointManager { get; }
        public IThreadEventsListener ThreadManager => _threadManager;
        
        public void WaitReadyToRun()
        {
            if (_isStarted)
                return;

            _startEvent.Wait();
            _isStarted = true;
        }

        internal void SetReadyToRun()
        {
            if (_isStarted)
                return;

            _startEvent.Set();
        }

        public event Action<DebugSession> OnClose;
        
        private static ThreadStopReason ConvertStopReason(MachineStopReason reason) => reason switch
        {
            MachineStopReason.Breakpoint => ThreadStopReason.Breakpoint,
            MachineStopReason.BreakpointConditionError => ThreadStopReason.Breakpoint,
            MachineStopReason.Step => ThreadStopReason.Step,
            MachineStopReason.Exception => ThreadStopReason.Exception,
            _ => throw new NotImplementedException(),
        };
    }
}