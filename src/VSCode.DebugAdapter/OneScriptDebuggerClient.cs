/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Runtime.CompilerServices;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using Serilog;
using VSCode.DebugAdapter.Transport;

namespace VSCode.DebugAdapter
{
    public class OneScriptDebuggerClient : IDebuggerService
    {
        private readonly IDebugEventListener _eventBackChannel;
        private readonly IMessageChannel _commandsChannel;
        private RpcProcessor _processor;
        
        private readonly ILogger Log = Serilog.Log.ForContext<OneScriptDebuggerClient>();
        
        public OneScriptDebuggerClient(
            IMessageChannel commandsChannel,
            IDebugEventListener eventBackChannel,
            int protocolVersion)
        {
            _commandsChannel = commandsChannel;
            _eventBackChannel = eventBackChannel;
            ProtocolVersion = protocolVersion;
        }
        
        public int ProtocolVersion { get; }

        public void Start()
        {
            RunEventsListener(_commandsChannel);
        }
        
        public void Stop()
        {
            _processor.Stop();
        }

        private void RunEventsListener(IMessageChannel channelToListen)
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
            Log.Debug("Debuggee event listener started");
        }

        private void WriteCommand<T>(T data, [CallerMemberName] string command = "")
        {
            Log.Verbose("Sending {Command} to debuggee, param {@Parameter}", command, data); 
            var dto = RpcCall.Create(nameof(IDebuggerService), command, data);
            _commandsChannel.Write(dto);
            Log.Verbose("Successfully written: {Command}", command);

        }
        
        private void WriteCommand(object[] data, [CallerMemberName] string command = "")
        {
            Log.Verbose("Sending {Command} to debuggee, params {@Parameters}", command, data);
            var dto = RpcCall.Create(nameof(IDebuggerService), command, data);
            _commandsChannel.Write(dto);
            Log.Verbose("Successfully written: {Command}", command);
        }
        
        private T GetResponse<T>()
        {
            var rpcResult = _processor.GetResult();
            Log.Verbose("Response received {Result} = {@Value}", rpcResult.Id, rpcResult.ReturnValue);
            if (rpcResult.ReturnValue is RpcExceptionDto excDto)
            {
                Log.Verbose("RPC Exception received: {Description}", excDto.Description);
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

        public void SetExceptionBreakpoints(ExceptionBreakpointFilter[] filters)
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
            Stop();
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