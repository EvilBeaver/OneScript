/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using OneScript.DebugProtocol;
using ScriptEngine;
using ScriptEngine.Machine;

namespace oscript.DebugServer
{
    internal class  TcpDebugController : DebugControllerBase
    {
        private TcpEventCallbackChannel _eventChannel;
        private TcpChannel _commandsChannel;

        public TcpDebugController(int listenerPort)
        {
            Port = listenerPort;
        }

        private int Port { get; }
        
        public override void Init()
        {
            base.Init();
            AcceptInitialConnections();
            RunCommandsLoop();

            SystemLogger.Write("Debug started");
        }

        private void RunCommandsLoop()
        {
            var incomingCommands = new Thread(() =>
            {
                bool shouldAcceptCommands = true;
                var debugService = new DefaultDebugService(this);
                while (shouldAcceptCommands)
                {
                    try
                    {
                        var data = _commandsChannel.Read<TcpProtocolDto>();
                        DispatchCommand(debugService, data);
                    }
                    catch (IOException)
                    {
                        shouldAcceptCommands = false;
                    }
                    catch (SocketException)
                    {
                        shouldAcceptCommands = false;
                    }
                }
            });

            incomingCommands.Start();
        }

        public override void NotifyProcessExit(int exitCode)
        {
            base.NotifyProcessExit(exitCode);
            _eventChannel.ProcessExited(exitCode);
            _commandsChannel.Dispose();
        }

        private void AcceptInitialConnections()
        {
            var listener = TcpListener.Create(Port);
            listener.Start();
            SystemLogger.Write("Initializing debugger");
            try
            {
                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    var channel = new TcpChannel(client);
                    var data = channel.Read<string>();
                    if (data == DebugChannelName.Commands && _commandsChannel == null)
                    {
                        _commandsChannel = channel;
                    }

                    if (data == DebugChannelName.Events && _eventChannel == null)
                    {
                        _eventChannel = new TcpEventCallbackChannel(channel);
                    }

                    if (_commandsChannel != null && _eventChannel != null)
                    {
                        break;
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        private void DispatchCommand(IDebuggerService debugService, TcpProtocolDto data)
        {
            switch (data.Id)
            {
                case nameof(debugService.Execute):
                {
                    debugService.Execute((int) data.Parameters[0]);
                    break;
                }
                case nameof(debugService.SetMachineBreakpoints):
                {
                    var breakpoints = debugService.SetMachineBreakpoints((Breakpoint[]) data.Parameters[0]);
                    _commandsChannel.Write(breakpoints);
                    break;
                }
                case nameof(debugService.GetStackFrames):
                {
                    var frames = debugService.GetStackFrames((int) data.Parameters[0]);
                    _commandsChannel.Write(frames);
                    break;
                }
                case nameof(debugService.GetVariables):
                {
                    var variables = debugService.GetVariables(
                        (int)data.Parameters[0],
                        (int)data.Parameters[1],
                        (int[])data.Parameters[2]);
                    _commandsChannel.Write(variables);
                    break;
                }
                case nameof(debugService.GetEvaluatedVariables):
                {
                    var variables = debugService.GetEvaluatedVariables(
                        (string)data.Parameters[0],
                        (int)data.Parameters[1],
                        (int)data.Parameters[2],
                        (int[])data.Parameters[3]);
                    
                    _commandsChannel.Write(variables);
                    break;
                }
                case nameof(debugService.Evaluate):
                {
                    var evResult = debugService.Evaluate(
                        (int)data.Parameters[0],
                        (int)data.Parameters[1],
                        (string)data.Parameters[2]);
                    _commandsChannel.Write(evResult);
                    break;
                }
                case nameof(debugService.Next):
                {
                    debugService.Next((int)data.Parameters[0]);
                    break;
                }
                case nameof(debugService.StepIn):
                {
                    debugService.StepIn((int)data.Parameters[0]);
                    break;
                }
                case nameof(debugService.StepOut):
                {
                    debugService.StepOut((int)data.Parameters[0]);
                    break;
                }
                case nameof(debugService.GetThreads):
                {
                    var threads = debugService.GetThreads();
                    _commandsChannel.Write(threads);
                    break;
                }
                default:
                    SystemLogger.Write($"Unknown protocol command:{data.Id}");
                    break;
            }
        }

        protected override void OnMachineStopped(MachineInstance machine, MachineStopReason reason)
        {
            var handle = GetTokenForThread(Thread.CurrentThread.ManagedThreadId);
            handle.ThreadEvent.Reset();
            _eventChannel.ThreadStopped(1, ConvertStopReason(reason));
            handle.ThreadEvent.Wait();
        }

        protected override void Dispose(bool disposing)
        {
            _commandsChannel?.Dispose();
            _eventChannel?.Dispose();
            base.Dispose(disposing);
            _commandsChannel = null;
            _eventChannel = null;
        }
    }
}