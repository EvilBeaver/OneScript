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
    /// <summary>
    /// Простой односессионный контроллер отладки. Поддерживает только один сеанс отладки на процесс.
    /// Также поддерживает только один BSL-процесс на приложение. При получении NotifyProcessExited отключает отладчик
    /// и к нему нельзя подключиться еще раз.
    /// </summary>
    public class DefaultDebugController : IDebugController
    {
        private readonly ICommunicationServer _server;
        private readonly IDebuggerService _debugger;
        private readonly IDebugEventListener _callbackService;
        private readonly ThreadManager _threadManager;

        public DefaultDebugController(ICommunicationServer ipcServer,
            IDebuggerService debugger,
            IDebugEventListener callbackService,
            ThreadManager threadManager, 
            IBreakpointManager breakpointManager)
        {
            _server = ipcServer;
            _debugger = debugger;
            _callbackService = callbackService;
            _threadManager = threadManager;
            BreakpointManager = breakpointManager;
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
            _callbackService.ThreadStopped(e.ThreadId, ConvertStopReason(e.StopReason), e.ErrorMessage);
            token.Wait();
        }

        public void Wait()
            => _threadManager.GetTokenForCurrentThread().Wait();

        public void NotifyProcessExit(int exitCode)
        {
            _threadManager.DetachFromCurrentThread();
            _callbackService.ProcessExited(exitCode);
            _server.Stop();
            
        }

        public void AttachToThread()
        {
            MachineInstance.Current.SetDebugMode(BreakpointManager);
            _threadManager.AttachToCurrentThread(); 
        }
        
        public void DetachFromThread()
            => _threadManager.DetachFromCurrentThread();

        public IBreakpointManager BreakpointManager { get; }

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