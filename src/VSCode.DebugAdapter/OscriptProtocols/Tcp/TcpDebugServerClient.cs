/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using Serilog;
using VSCode.DebugAdapter.OscriptProtocols;

namespace VSCode.DebugAdapter
{
    public class TcpDebugServerClient : IDebuggerService
    {
        private readonly int _port;
        private readonly IDebugEventListener _eventBackChannel;
        private BinaryChannel _commandsChannel;
        private RpcProcessor _processor;
        
        public TcpDebugServerClient(int port, IDebugEventListener eventBackChannel)
        {
            _port = port;
            _eventBackChannel = eventBackChannel;
        }
        
        public void Connect()
        {
            var debuggerUri = GetDebuggerUri(_port); 
            
            var client = new TcpClient();
            TryConnect(client, debuggerUri);
            _commandsChannel = new BinaryChannel(client);
            
            Log.Debug("Connected to {Host}:{Port}", debuggerUri.Host, debuggerUri.Port);

            RunEventsListener(_commandsChannel);
        }

        public void Disconnect()
        {
            _processor.Stop();
            _commandsChannel.Dispose();
        }

        private static Uri GetDebuggerUri(int port)
        {
            var builder = new UriBuilder();
            builder.Scheme = "net.tcp";
            builder.Port = port;
            builder.Host = "localhost";

            return builder.Uri;
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
                    
                    Log.Warning("Error. Retry connect {Attempt}", i);
                    Thread.Sleep(1500);
                }
            }
        }

        private void RunEventsListener(ICommunicationChannel channelToListen)
        {
            var server = new DefaultMessageServer<TcpProtocolDtoBase>(channelToListen);
            server.ServerThreadName = "dbg-client-event-listener";
            
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
            Log.Verbose("Sending {Command} to debuggee, param {Parameter}", command, data); 
            var dto = RpcCall.Create(nameof(IDebuggerService), command, data);
            _commandsChannel.Write(dto);
            Log.Verbose("Successfully written: {Command}", command);

        }
        
        private void WriteCommand(object[] data, [CallerMemberName] string command = "")
        {
            Log.Verbose("Sending {Command} to debuggee, params {Parameters}", command, data);
            var dto = RpcCall.Create(nameof(IDebuggerService), command, data);
            _commandsChannel.Write(dto);
            Log.Verbose("Successfully written: {Command}", command);
        }
        
        private T GetResponse<T>()
        {
            var rpcResult = _processor.GetResult();
            Log.Debug("Response received {Result} = {Value}", rpcResult.Id, rpcResult.ReturnValue);
            if (rpcResult.ReturnValue is RpcExceptionDto excDto)
            {
                Log.Debug("RPC Exception received: {Description}", excDto.Description);
                throw new RpcOperationException(excDto);
            }
            
            return (T) rpcResult.ReturnValue;
        }
        
        public void Execute(int threadId)
        {
            WriteCommand(threadId);
        }

        public void SetMachineExceptionBreakpoints((string Id, string Condition)[] filters)
        {
            WriteCommand(filters);
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

        public void Disconnect(bool terminate)
        {
            WriteCommand(terminate);
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

        public int GetProtocolVersion()
        {
            WriteCommand(null);
            try
            {
                return ProtocolVersions.Adjust(GetResponse<int>());
            }
            catch (RpcOperationException e)
            {
                Log.Information("Checking version returned error: {Err}", e.Message);
                return ProtocolVersions.SafestVersion;
            }
        }
    }
}