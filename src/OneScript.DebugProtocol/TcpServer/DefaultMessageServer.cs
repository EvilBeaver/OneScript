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

        public DefaultMessageServer(ICommunicationChannel protocolChannel)
        {
            _protocolChannel = protocolChannel;
        }

        public void Start()
        {
            RunCommandsLoop();
        }
        
        private void RunCommandsLoop()
        {
            _messageThread = new Thread(() =>
            {
                bool shouldAcceptCommands = true;
                while (shouldAcceptCommands)
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
                        var eventData = new CommunicationEventArgs
                        {
                            Data = null,
                            Channel = _protocolChannel,
                            Exception = e
                        };
                        
                        DataReceived?.Invoke(this, eventData);
                        
                        // свойство в исключении может быть утановлено в обработчике евента
                        shouldAcceptCommands = e.StopChannel;
                    }
                    catch (ThreadInterruptedException)
                    {
                        shouldAcceptCommands = false;
                    }
                    catch (Exception)
                    {
                        shouldAcceptCommands = false;
                    }
                }
            });

            _messageThread.Start();
        }

        public void Stop()
        {
            if (_messageThread?.IsAlive == true)
            {
                _protocolChannel.Dispose();
                _messageThread.Interrupt();
            }
        }

        public event EventHandler<CommunicationEventArgs> DataReceived;
    }
}