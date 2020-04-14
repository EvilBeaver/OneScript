/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    public class DefaultDebugController : IDebugController
    {
        private readonly ICommunicationServer _server;
        private readonly IDebuggerService _debugger;
        private readonly IDebugEventListener _callbackService;
        private readonly ThreadManager _threadManager;

        public DefaultDebugController(
            ICommunicationServer ipcServer,
            IDebuggerService debugger,
            IDebugEventListener callbackService,
            ThreadManager threadManager)
        {
            _server = ipcServer;
            _debugger = debugger;
            _callbackService = callbackService;
            _threadManager = threadManager;
        }

        public void Dispose()
        {
            _server.Stop();
            _threadManager.Dispose();
        }

        public void Init()
        {
            _threadManager.ThreadStopped += ThreadManagerOnThreadStopped;
            var dispatcher = new DispatchingService<IDebuggerService>(_server, _debugger);
            dispatcher.Start();
        }

        private void ThreadManagerOnThreadStopped(object sender, ThreadStoppedEventArgs e)
        {
            var token = _threadManager.GetTokenForThread(e.ThreadId);
            token.Reset();
            _callbackService.ThreadStopped(e.ThreadId, ConvertStopReason(e.StopReason));
            token.Wait();
        }

        public void Wait()
        {
            _threadManager.GetTokenForCurrentThread().Wait();
        }

        public void NotifyProcessExit(int exitCode)
        {
            _threadManager.DetachFromCurrentThread();
            _callbackService.ProcessExited(exitCode);
            
        }

        public void AttachToThread(MachineInstance machine)
        {
            _threadManager.AttachToCurrentThread(); 
        }
        
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
    }
}