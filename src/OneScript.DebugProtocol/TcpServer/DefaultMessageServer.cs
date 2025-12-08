/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Threading;
using OneScript.DebugProtocol.Abstractions;

namespace OneScript.DebugProtocol.TcpServer
{
    /// <summary>
    /// Читает сообщения из канала и вызывает обработчики сообщений.
    /// Управляет жизненным циклом канала и освобождает его ресурсы при завершении работы сервера.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class DefaultMessageServer<TMessage> : ICommunicationServer
    {
        private readonly IMessageChannel _protocolChannel;
        private readonly object _disposeLock = new object();
        private Thread _messageThread;
        private volatile bool _serverStopped;
        private bool _channelDisposed;

        public DefaultMessageServer(IMessageChannel protocolChannel)
        {
            _protocolChannel = protocolChannel;
        }
        
        /// <summary>
        /// Имя, назначаемое потоку сервера. Полезно для отладки и диагностики.
        /// </summary>
        public string ServerThreadName { get; set; }

        public void Start()
        {
            RunCommandsLoop();
        }
        
        private void RunCommandsLoop()
        {
            _messageThread = new Thread(() =>
            {
                _serverStopped = false;
                while (!_serverStopped)
                {
                    try
                    {
                        var data = _protocolChannel.Read<TMessage>();
                        var eventData = new CommunicationEventArgs
                        {
                            Data = data,
                            Channel = _protocolChannel,
                        };

                        DataReceived?.Invoke(this, eventData);
                    }
                    catch (ChannelException e)
                    {
                        if (e.StopChannel)
                        {
                            // критичные исключения сразу должны завершать сервер
                            _serverStopped = true;
                            break;
                        }

                        var eventData = new CommunicationEventArgs
                        {
                            Data = null,
                            Channel = _protocolChannel,
                            Exception = e
                        };

                        try
                        {
                            DataReceived?.Invoke(this, eventData);
                        }
                        catch
                        {
                            // один из обработчиков выбросил исключение
                            // мы все равно не знаем что с ним делать.

                            // Считаем, что факап подписчика - его проблемы.
                        }

                        // свойство в исключении может быть уcтановлено в обработчике евента
                        _serverStopped = e.StopChannel;
                    }
                    catch (ObjectDisposedException)
                    {
                        _serverStopped = true;
                    }
                    catch (ThreadInterruptedException)
                    {
                        // Сервер принудительно остановлен
                        _serverStopped = true;
                    }
                    catch (Exception e)
                    {
                        if (OnError == null)
                        {
                            _serverStopped = true;
                            break;
                        }
                        
                        var eventData = new CommunicationEventArgs
                        {
                            Data = null,
                            Channel = _protocolChannel,
                            Exception = new ChannelException("Unhandled error in message handler", true, e)
                        };

                        OnError?.Invoke(this, eventData);
                    }
                }
                
                DisposeChannel();
            });
            
            _messageThread.IsBackground = true;
            if (ServerThreadName != default)
            {
                _messageThread.Name = ServerThreadName;
            }

            _messageThread.Start();
        }

        private void DisposeChannel()
        {
            lock (_disposeLock)
            {
                if (_channelDisposed)
                    return;
                
                _channelDisposed = true;
                _protocolChannel.Dispose();
            }
        }

        public void Stop()
        {
            if (_serverStopped)
                return;
            
            _serverStopped = true;

            if (_messageThread?.IsAlive == true)
            {
                _messageThread.Interrupt();
            }
        }

        public event EventHandler<CommunicationEventArgs> DataReceived;
        public event EventHandler<CommunicationEventArgs> OnError;
    }
}