/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Net.Sockets;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using OneScript.DebugServices.Internal;
using ScriptEngine.Machine;

namespace OneScript.DebugServices
{
    /// <summary>
    /// Контроллер отладки с поддержкой переподключения. Позволяет отключиться от сеанса отладки
    /// и подключиться заново без перезапуска процесса.
    /// </summary>
    public class ReconnectableDebugController : IDebugController
    {
        private readonly int _port;
        private readonly object _lock = new object();
        
        private TcpListener _listener;
        private ICommunicationServer _server;
        private DefaultDebugService _debugger;
        private IDebugEventListener _callbackService;
        private ThreadManager _threadManager;
        private DefaultBreakpointManager _breakpointManager;
        private DispatchingService<IDebuggerService> _dispatcher;
        private DelayedConnectionChannel _channel;
        
        private bool _disposed;
        private bool _initialized;

        public ReconnectableDebugController(int port)
        {
            _port = port;
        }

        public IBreakpointManager BreakpointManager => _breakpointManager;
        public IThreadManager ThreadManager => _threadManager;

        public void Init()
        {
            lock (_lock)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ReconnectableDebugController));

                if (_initialized)
                {
                    DisposeSession();
                }

                CreateSession();
                _initialized = true;
            }
        }

        private void CreateSession()
        {
            _listener = TcpListener.Create(_port);
            _channel = new DelayedConnectionChannel(_listener);
            _server = new DefaultMessageServer<RpcCall>(_channel)
            {
                ServerThreadName = "debug-server"
            };
            _callbackService = new TcpEventCallbackChannel(_channel);
            _threadManager = new ThreadManager();
            _breakpointManager = new DefaultBreakpointManager();
            _debugger = new DefaultDebugService(_breakpointManager, _threadManager, new DefaultVariableVisualizer());
            
            // Subscribe to disconnect events
            _debugger.Disconnected += OnDebuggerDisconnected;
            
            _threadManager.ThreadStopped += ThreadManagerOnThreadStopped;
            _dispatcher = new DispatchingService<IDebuggerService>(_server, _debugger);
            _dispatcher.Start();
        }

        private void OnDebuggerDisconnected(object sender, DisconnectEventArgs e)
        {
            if (!e.Terminate)
            {
                // In attach mode, prepare for reconnection
                lock (_lock)
                {
                    if (!_disposed)
                    {
                        DisposeSession();
                        // Recreate session for next connection
                        try
                        {
                            CreateSession();
                        }
                        catch (Exception)
                        {
                            // Failed to recreate session, but we're in a background thread
                            // so we can't do much about it
                        }
                    }
                }
            }
        }

        private void DisposeSession()
        {
            if (_debugger != null)
            {
                _debugger.Disconnected -= OnDebuggerDisconnected;
            }
            
            if (_threadManager != null)
            {
                _threadManager.ThreadStopped -= ThreadManagerOnThreadStopped;
            }

            _dispatcher?.Stop();
            _server?.Stop();
            _threadManager?.Dispose();
            _channel?.Dispose();
            _callbackService?.Dispose();
            
            if (_listener != null)
            {
                try
                {
                    _listener.Stop();
                }
                catch
                {
                    // Listener may already be stopped
                }
            }
            
            _channel = null;
            _callbackService = null;
            _server = null;
            _dispatcher = null;
            _debugger = null;
            _threadManager = null;
            _breakpointManager = null;
            _listener = null;
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
            
            _callbackService.ThreadStoppedEx(e.ThreadId, ConvertStopReason(e.StopReason), e.ErrorMessage);
            token.Wait();
        }

        public void Wait()
        {
            lock (_lock)
            {
                if (_debugger == null)
                    throw new InvalidOperationException("Controller not initialized");
                    
                _debugger.WaitForExecution();
            }
        }

        public void NotifyProcessExit(int exitCode)
        {
            lock (_lock)
            {
                if (_threadManager != null)
                {
                    _threadManager.ReleaseAllThreads();
                }
                
                if (_callbackService != null)
                {
                    _callbackService.ProcessExited(exitCode);
                }
                
                DisposeSession();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;
                
                DisposeSession();
                
                _disposed = true;
            }
        }

        private static ThreadStopReason ConvertStopReason(MachineStopReason reason) => reason switch
        {
            MachineStopReason.Breakpoint => ThreadStopReason.Breakpoint,
            MachineStopReason.BreakpointConditionError => ThreadStopReason.Breakpoint,
            MachineStopReason.Step => ThreadStopReason.Step,
            MachineStopReason.Exception => ThreadStopReason.Exception,
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Event arguments for debugger disconnect events
    /// </summary>
    public class DisconnectEventArgs : EventArgs
    {
        public bool Terminate { get; set; }
    }
}
