/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.DebugProtocol;
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
            IDebugEventListener callbackService)
        {
            _server = ipcServer;
            _debugger = debugger;
            _callbackService = callbackService;
            _threadManager = new ThreadManager();
        }

        public void Dispose()
        {
            _server.Stop();
            _server.DataReceived -= ServerOnDataReceived;
            _threadManager.Dispose();
        }

        public void Init()
        {
            _server.DataReceived += ServerOnDataReceived; 
            _server.Start();
        }

        private void ServerOnDataReceived(object sender, CommunicationEventArgs e)
        {
            throw new System.NotImplementedException();
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
    }
}