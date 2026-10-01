/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OneScript.DebugProtocol.Abstractions;

namespace OneScript.DebugServices
{
    public class TcpDebugServer : IDebugServer
    {
        private readonly int _port;
        private volatile bool _isListening;
        private TcpListener _listener;

        public TcpDebugServer(int port)
        {
            _port = port;
        }

        public enum LogLevel
        {
            Debug,
            Info,
            Error
        }

        public int ActualPort() => (_listener.LocalEndpoint as IPEndPoint)?.Port ?? 0;
        
        public delegate void LogHandler(LogLevel level, string message);
        
        public event LogHandler OnLogEvent;
        
        public void Listen()
        {
            if (_isListening)
                return;
            
            _listener = TcpListener.Create(_port);

            _listener.Start();
            _isListening = true;
            
            StartListenerThread();
        }

        private void StartListenerThread()
        {
            new Thread(ListenLoop)
            {
                IsBackground = true,
                Name = "debug-server-listener",
            }.Start();
        }

        private void ListenLoop()
        {
            OnLogEvent?.Invoke(LogLevel.Info, "Starting listener thread");
            while (_isListening)
            {
                try
                {
                    // Stop() из другого потока обнуляет поле
                    var listener = _listener;
                    if (listener == null)
                        break;

                    AcceptClient(listener);
                }
                catch (ObjectDisposedException)
                {
                    _isListening = false;
                    OnLogEvent?.Invoke(LogLevel.Debug, "Listener interrupted");
                    break;
                }
                catch (InvalidOperationException) when (!_isListening)
                {
                    // Stop() успел остановить слушателя между проверкой и AcceptTcpClient
                    break;
                }
                catch (SocketException e)
                {
                    if (!HandleSocketException(e))
                        break;
                }
            }
            OnLogEvent?.Invoke(LogLevel.Info, "Listener thread stopped");
        }

        private void AcceptClient(TcpListener listener)
        {
            var client = listener.AcceptTcpClient();
            OnLogEvent?.Invoke(LogLevel.Debug, "Client connected");
            try
            {
                OnClientConnected?.Invoke(this, new TcpDebuggerClient(client));
            }
            catch (Exception e)
            {
                // Клиент закрылся до рукопожатия или прислал мусор (проверка порта и т.п.):
                // отлаживаемый процесс из-за этого падать не должен
                OnLogEvent?.Invoke(LogLevel.Error, $"Client connection failed: {e}");
                client.Dispose();
            }
        }

        // false - цикл приема пора прервать
        private bool HandleSocketException(SocketException e)
        {
            if (_isListening == false)
            {
                // Сервер находится в процессе остановки внешним потоком
                // и ошибка сокета не является ошибкой.
                return false;
            }

            OnLogEvent?.Invoke(LogLevel.Error, $"Socket exception: {e}");
            if (OnListenException == null)
            {
                Stop();
                return false;
            }

            var args = new ListenerErrorEventArgs(e);
            try
            {
                OnLogEvent?.Invoke(LogLevel.Debug, $"Calling exception handler");
                OnListenException?.Invoke(this, args);
                if (args.StopServer)
                {
                    Stop();
                }
            }
            catch (Exception handlerException)
            {
                OnLogEvent?.Invoke(LogLevel.Error, $"Exception handler raised exception {handlerException}");
                Stop();
                throw;
            }

            return true;
        }

        private class TcpDebuggerClient : IDebuggerClient
        {
            private TcpClient Client { get; }

            public TcpDebuggerClient(TcpClient client)
            {
                Client = client;
            }

            public void Dispose()
            {
                Client.Dispose();
            }

            public Stream GetDataStream()
            {
                return Client.GetStream();
            }

            public bool Connected => Client.Connected;
        }

        public void Stop()
        {
            OnLogEvent?.Invoke(LogLevel.Debug, "Stopping listener");
            _isListening = false;
            try
            {
                _listener?.Stop();
            }
            catch (Exception e)
            {
                // тут нечего делать
                OnLogEvent?.Invoke(LogLevel.Error, $"Exception while stopping listener: {e}");
            }
            finally
            {
                _listener = null;
            }
                
        }

        public event EventHandler<IDebuggerClient> OnClientConnected;
        public event EventHandler<ListenerErrorEventArgs> OnListenException;
    }
}