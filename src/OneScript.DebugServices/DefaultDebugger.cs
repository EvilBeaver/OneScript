/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Diagnostics;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using OneScript.DebugServices.Internal;
using ScriptEngine.Machine.Debugger;

namespace OneScript.DebugServices
{
    /// <summary>
    /// Простой отладчик, поддерживающий одновременно только одну сессию отладки.
    /// </summary>
    public class DefaultDebugger : IDebugger
    {
        // NB! должен быть согласован с перечислением TransportProtocols в адаптере
        private const short JSON_FORMAT_MARKER = 2;
        // NB! должен быть согласован с файлом ProtocolVersions в адаптере
        private const short SUPPORTED_FORMAT_VERSION = 3;
        
        private readonly IDebugServer _transport;
        private IDebugSession _session;
        
        private readonly object _sessionRecycleLock = new object();

        public DefaultDebugger(IDebugServer transport)
        {
            _transport = transport;
        }

        public bool IsEnabled => true;
        public bool AttachMode { get; set; }

        public void Start()
        {
            _transport.OnClientConnected += TransportOnOnClientConnected;
            _transport.OnListenException += ListenerException;
            _transport.Listen();
        }

        private void ListenerException(object sender, ListenerErrorEventArgs e)
        {
            e.StopServer = true;
        }

        private void TransportOnOnClientConnected(object sender, IDebuggerClient debuggerClient)
        {
            lock (_sessionRecycleLock)
            {
                if (_session?.IsActive == true)
                {
                    // Если есть активная сессия, то не начинаем новую
                    debuggerClient.Dispose();
                    return;
                }
            }

            var dataStream = debuggerClient.GetDataStream();
            if (FormatReconcileUtils.CheckReconcileRequest(dataStream))
            {
                // Да, это наш фейковый заголовок
                FormatReconcileUtils.WriteReconcileResponse(dataStream, JSON_FORMAT_MARKER, SUPPORTED_FORMAT_VERSION);
            }
            else
            {
                // Не поддерживаем старый формат протокола отладки
                debuggerClient.Dispose();
                return;
            }

            lock (_sessionRecycleLock)
            {
                var session = new DebugSession(debuggerClient, AttachMode);
                session.OnClose += OnSessionClose;

                if (_session is ConnectableSessionProxy proxy)
                {
                    proxy.Connect(session);
                }
                else
                {
                    _session = session;
                }
            }
        }

        private void OnSessionClose(DebugSession session)
        {
            lock (_sessionRecycleLock)
            {
                session.OnClose -= OnSessionClose;
                _session = null;
            }
        }

        public IDebugSession GetSession()
        {
            lock (_sessionRecycleLock)
            {
                return _session ??= new ConnectableSessionProxy(AttachMode);
            }
        }

        public void NotifyProcessExit(int exitCode)
        {
            _transport.OnClientConnected-=TransportOnOnClientConnected;
            _transport.Stop();

            _session?.Dispose();
        }
    }
}