/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Threading;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;

namespace VSCode.DebugAdapter
{
    public class RpcProcessor
    {
        private readonly ICommunicationServer _server;
        private readonly Dictionary<string, ChannelRecord> _dispatchers = new Dictionary<string, ChannelRecord>();
        private readonly Queue<RpcCallResult> _responses = new Queue<RpcCallResult>();
        private readonly AutoResetEvent _responseAvailable = new AutoResetEvent(false);

        private struct ChannelRecord
        {
            public IMethodsDispatcher Dispatcher;
            public object ServiceInstance;
        }
        
        public RpcProcessor(ICommunicationServer server)
        {
            _server = server;
        }

        public void Start()
        {
            _server.DataReceived += ServerOnDataReceived;
            _server.Start();
        }
        
        public void Stop()
        {
            _server.DataReceived -= ServerOnDataReceived;
            _server.Stop();
        }
        
        private void ServerOnDataReceived(object sender, CommunicationEventArgs e)
        {
            SessionLog.WriteLine("Data received " + e.Data);
            if (e.Exception == null)
            {
                DispatchMessage((TcpProtocolDtoBase)e.Data, e.Channel);
            }
            else
            {
                SessionLog.WriteLine(e.Exception.ToString());
                if (e.Exception.StopChannel)
                {
                    Stop();
                }
            }
        }

        private void DispatchMessage(TcpProtocolDtoBase data, ICommunicationChannel responseChannel)
        {
            if (!_dispatchers.TryGetValue(data.ServiceName, out var serviceRecord))
            {
                SessionLog.WriteLine("No dispatcher for " + data.ServiceName);
                return;
            }

            if (data is RpcCall rpcCall)
            {
                SessionLog.WriteLine("Processing call to " + rpcCall.Id);
                var result = serviceRecord.Dispatcher.Dispatch(serviceRecord.ServiceInstance, rpcCall.Id, rpcCall.Parameters);
                if (result != null)
                {
                    var callResult = RpcCallResult.Respond(rpcCall, result);
                    responseChannel.Write(callResult);
                }
            }
            else if(data is RpcCallResult result)
            {
                SessionLog.WriteLine("Saving response to " + result.Id);
                lock (_responses)
                {
                    _responses.Enqueue(result);
                }
                _responseAvailable.Set();
            }
        }

        public void AddChannel(string channelName, Type serviceType, object instance)
        {
            var type = instance.GetType();
            if (!serviceType.IsAssignableFrom(type))
            {
                throw new ArgumentException($"Instance must implement {serviceType}");
            }

            var dispatcherType = typeof(MethodsDispatcher<>);
            var dispatcherConcreteType = dispatcherType.MakeGenericType(serviceType);
            var dispatcher = (IMethodsDispatcher)Activator.CreateInstance(dispatcherConcreteType);
            _dispatchers.Add(channelName, new ChannelRecord
            {
                Dispatcher = dispatcher,
                ServiceInstance = instance
            });
        }

        public RpcCallResult GetResult()
        {
            _responseAvailable.WaitOne();
            lock (_responses)
            {
                return _responses.Dequeue();
            }
        }
    }
}