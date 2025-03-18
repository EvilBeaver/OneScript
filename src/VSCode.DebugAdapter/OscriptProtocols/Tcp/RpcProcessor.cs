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
using Serilog;

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
            _server.OnError += ServerOnServerExited; 
            _server.Start();
        }

        private void ServerOnServerExited(object sender, CommunicationEventArgs e)
        {
            Log.Fatal(e.Exception, "Server error");
            Stop();
        }

        public void Stop()
        {
            _server.DataReceived -= ServerOnDataReceived;
            _server.OnError -= ServerOnServerExited;
            _server.Stop();
        }
        
        private void ServerOnDataReceived(object sender, CommunicationEventArgs e)
        {
            if (e.Exception == null)
            {
                Log.Debug("Data received {@Data}", (TcpProtocolDtoBase)e.Data);
                DispatchMessage((TcpProtocolDtoBase)e.Data, e.Channel);
            }
            else
            {
                Log.Error(e.Exception, "RPC Exception received. Critical: {Critical}", e.Exception.StopChannel);
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
                Log.Warning("No dispatcher for {ServiceName}", data.ServiceName);
                return;
            }

            if (data is RpcCall rpcCall)
            {
                Log.Debug("Processing call to {Id}", rpcCall.Id);
                var result = serviceRecord.Dispatcher.Dispatch(serviceRecord.ServiceInstance, rpcCall.Id, rpcCall.Parameters);
                Log.Debug("Completed call to {Id}", rpcCall.Id);
                if (result != null)
                {
                    var callResult = RpcCallResult.Respond(rpcCall, result);
                    Log.Debug("Sending response {Result}", callResult.ReturnValue);
                    responseChannel.Write(callResult);
                }
            }
            else if(data is RpcCallResult result)
            {
                Log.Debug("Enque response to {Id}. Value {Value}", result.Id, result.ReturnValue);
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