/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;

namespace VSCode.DebugAdapter
{
    public class TcpDebugConnector : IDebuggerService
    {
        private readonly int _port;
        private readonly IDebugEventListener _eventBackChannel;
        private BinaryChannel _commandsChannel;
        private RpcProcessor _processor;

        public TcpDebugConnector(int port, IDebugEventListener eventBackChannel)
        {
            _port = port;
            _eventBackChannel = eventBackChannel;
        }
        
        public void Connect()
        {
            var debuggerUri = Binder.GetDebuggerUri(_port); 
            
            SessionLog.WriteLine("Creating commands tcp channel");

            var client = new TcpClient();
            TryConnect(client, debuggerUri);
            _commandsChannel = new BinaryChannel(client);
            
            SessionLog.WriteLine("connected");

            RunEventsListener(_commandsChannel);
        }

        private static void TryConnect(TcpClient client, Uri debuggerUri)
        {
            const int limit = 3;
            // TODO: параметризовать ожидания и попытки
            for (int i = 0; i < limit; ++i)
            {
                try
                {
                    client.Connect(debuggerUri.Host, debuggerUri.Port);
                    break;
                }
                catch (SocketException)
                {
                    if (i == limit - 1)
                        throw;
                    
                    SessionLog.WriteLine("Error. Retry connect");
                    Thread.Sleep(1500);
                }
            }
        }

        private void RunEventsListener(ICommunicationChannel channelToListen)
        {
            var server = new DefaultMessageServer<TcpProtocolDtoBase>(channelToListen);
            
            _processor = new RpcProcessor(server);
            _processor.AddChannel(
                nameof(IDebugEventListener),
                typeof(IDebugEventListener),
                _eventBackChannel);
            
            _processor.AddChannel(
                nameof(IDebuggerService),
                typeof(IDebuggerService),
                this);
            
            _processor.Start();
        }

        private void WriteCommand<T>(T data, [CallerMemberName] string command = "")
        {
            SessionLog.WriteLine($"Sending {command} to debuggee"); 
            var dto = RpcCall.Create(nameof(IDebuggerService), command, data);
            _commandsChannel.Write(dto);

        }
        
        private void WriteCommand(object[] data, [CallerMemberName] string command = "")
        {
            SessionLog.WriteLine($"Sending {command} to debuggee"); 
            var dto = RpcCall.Create(nameof(IDebuggerService), command, data);
            _commandsChannel.Write(dto);
        }
        
        private T GetResponse<T>()
        {
            var rpcResult = _processor.GetResult();
            SessionLog.WriteLine("Response received " + rpcResult.Id + " t = " + rpcResult.ReturnValue);
            if (rpcResult.ReturnValue is RpcExceptionDto excDto)
            {
                SessionLog.WriteLine($"Exception received: {excDto.Description}");
                throw new RpcOperationException(excDto);
            }
            
            return (T) rpcResult.ReturnValue;
        }
        
        public void Execute(int threadId)
        {
            WriteCommand(threadId);
        }

        public Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet)
        {
            WriteCommand(breaksToSet);
            return GetResponse<Breakpoint[]>();
        }

        public StackFrame[] GetStackFrames(int threadId)
        {
            WriteCommand(threadId);
            return GetResponse<StackFrame[]>();
        }

        public Variable[] GetVariables(int threadId, int frameIndex, int[] path)
        {
            WriteCommand(new object[]
            {
                threadId,
                frameIndex,
                path
            });

            return GetResponse<Variable[]>();
        }

        public Variable[] GetEvaluatedVariables(string expression, int threadId, int frameIndex, int[] path)
        {
            WriteCommand(new object[]
            {
                expression,
                threadId,
                frameIndex,
                path
            });

            return GetResponse<Variable[]>();
        }

        public Variable Evaluate(int threadId, int contextFrame, string expression)
        {
            WriteCommand(new object[]
            {
                threadId,
                contextFrame,
                expression
            });

            return GetResponse<Variable>();
        }

        public void Next(int threadId)
        {
            WriteCommand(threadId);
        }

        public void StepIn(int threadId)
        {
            WriteCommand(threadId);
        }

        public void StepOut(int threadId)
        {
            WriteCommand(threadId);
        }

        public int[] GetThreads()
        {
            WriteCommand(null);
            return GetResponse<int[]>();
        }

        public int GetProcessId()
        {
            WriteCommand(null);
            return GetResponse<int>();
        }
    }
}