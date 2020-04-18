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

        public TcpDebugConnector(int port, IDebugEventListener eventBackChannel)
        {
            _port = port;
            _eventBackChannel = eventBackChannel;
        }
        
        public void Connect()
        {
            var debuggerUri = Binder.GetDebuggerUri(_port); 
            
            SessionLog.WriteLine("Creating commands tcp channel");
            _commandsChannel = new BinaryChannel(new TcpClient(debuggerUri.Host, debuggerUri.Port));
            _commandsChannel.Write(DebugChannelName.Commands);
            
            SessionLog.WriteLine("Creating events tcp channel");
            var eventsChannel = new BinaryChannel(new TcpClient(debuggerUri.Host, debuggerUri.Port));
            eventsChannel.Write(DebugChannelName.Events);
            
            SessionLog.WriteLine("connected");

            RunEventsListener(eventsChannel);
        }

        private void RunEventsListener(ICommunicationChannel channelToListen)
        {
            var server = new DefaultMessageServer<TcpProtocolDto>(channelToListen);
            var dispatcher = new DispatchingService<IDebugEventListener>(server, _eventBackChannel);
            dispatcher.Start();
        }

        private void WriteCommand<T>(T data, [CallerMemberName] string command = "")
        {
            SessionLog.WriteLine($"Sending {command} to debuggee"); 
            var dto = TcpProtocolDto.Create(command, data);
            _commandsChannel.Write(dto);
        }
        
        private void WriteCommand(object[] data, [CallerMemberName] string command = "")
        {
            SessionLog.WriteLine($"Sending {command} to debuggee"); 
            var dto = TcpProtocolDto.Create(command, data);
            _commandsChannel.Write(dto);
        }
        
        public void Execute(int threadId)
        {
            WriteCommand(threadId);
        }

        public Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet)
        {
            WriteCommand(breaksToSet);
            return _commandsChannel.Read<Breakpoint[]>();
        }

        public StackFrame[] GetStackFrames(int threadId)
        {
            WriteCommand(threadId);
            return _commandsChannel.Read<StackFrame[]>();
        }

        public Variable[] GetVariables(int threadId, int frameIndex, int[] path)
        {
            WriteCommand(new object[]
            {
                threadId,
                frameIndex,
                path
            });

            return _commandsChannel.Read<Variable[]>();
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

            return _commandsChannel.Read<Variable[]>();
        }

        public Variable Evaluate(int threadId, int contextFrame, string expression)
        {
            WriteCommand(new object[]
            {
                threadId,
                contextFrame,
                expression
            });

            return _commandsChannel.Read<Variable>();
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
            return _commandsChannel.Read<int[]>();
        }
    }
}