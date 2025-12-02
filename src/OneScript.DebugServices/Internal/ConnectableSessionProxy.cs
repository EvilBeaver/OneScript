/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Threading;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Debugger;

namespace OneScript.DebugServices.Internal
{
    internal class ConnectableSessionProxy : IDebugSession
    {
        private IDebugSession _session = new DisabledDebugSession();
        private bool _isConnected;
        private readonly ManualResetEventSlim _connectionEvent = new ManualResetEventSlim();

        public ConnectableSessionProxy(bool attachMode)
        {
            _isConnected = attachMode;
        }

        public void Dispose()
        {
            var activeSession = _session;
            
            _connectionEvent.Reset();
            _isConnected = false;
            _session = new DisabledDebugSession();
            
            activeSession.Dispose();
        }

        public void Connect(IDebugSession session)
        {
            _session = session;
            _isConnected = true;
            _connectionEvent.Set();
        }

        public IBreakpointManager BreakpointManager => _session.BreakpointManager;

        public IThreadEventsListener ThreadManager => _session.ThreadManager;

        public void WaitReadyToRun()
        {
            if (_isConnected)
            {
                _session.WaitReadyToRun(); // просто делегируем
                return;
            }

            _connectionEvent.Wait();
        }

        public bool IsActive => _session.IsActive;
    }
}