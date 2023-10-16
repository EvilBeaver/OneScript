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
    public class DefaultMessageServer<TMessage> : ICommunicationServer
    {
        private readonly ICommunicationChannel _protocolChannel;
        private Thread _messageThread;
        private volatile bool _serverStopped;

        public DefaultMessageServer(ICommunicationChannel protocolChannel)
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
                    catch (Exception)
                    {
                        _serverStopped = true;
                    }
                }
            });

            _messageThread.IsBackground = true;
            if (ServerThreadName != default)
            {
                _messageThread.Name = ServerThreadName;
            }

            _messageThread.Start();
        }

        public void Stop()
        {
            if (_serverStopped)
                return;
            
            _serverStopped = true;

            if (_messageThread?.IsAlive == true)
            {
                _protocolChannel.Dispose();
                _messageThread.Interrupt();
            }
        }

        public event EventHandler<CommunicationEventArgs> DataReceived;
    }
}