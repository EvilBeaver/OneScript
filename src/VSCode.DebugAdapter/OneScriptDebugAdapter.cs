// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EvilBeaver.DAP.Dto.Base;
using EvilBeaver.DAP.Dto.Events;
using EvilBeaver.DAP.Dto.Requests;
using EvilBeaver.DAP.Dto.Serialization;
using EvilBeaver.DAP.Dto.Types;
using EvilBeaver.DAP.Server;
using Microsoft.Extensions.Logging;
using VSCode.DebugAdapter.Transport;

namespace VSCode.DebugAdapter
{
    internal class OneScriptDebugAdapter : DebugAdapterBase
    {
        private DebugeeProcess _debuggee;
        private bool _startupPerformed = false;
        private ThreadStateContainer _threadState = new ThreadStateContainer();
        
        private ILogger Log { get; }

        public OneScriptDebugAdapter(ILogger logger)
        {
            Log = logger;
        }

        protected override async Task<InitializeResponse> OnInitializeAsync(InitializeRequest request, CancellationToken ct)
        {
            await EventsChannel.SendEventAsync(new InitializedEvent(), ct);
            
            return new InitializeResponse()
            {
                Body = new Capabilities
                {
                    SupportsConditionalBreakpoints = true,
                    SupportsFunctionBreakpoints = false,
                    SupportsConfigurationDoneRequest = true,
                    SupportsExceptionFilterOptions = true,
                    ExceptionBreakpointFilters = new []
                    {
                        new ExceptionBreakpointsFilter
                        {
                            Filter = "uncaught",
                            Label = "Необработанные исключения",
                            Description = "Остановка при возникновении необработанного исключения",
                            SupportsCondition = true,
                            ConditionDescription = "Искомая подстрока текста исключения"
                        },
                        new ExceptionBreakpointsFilter
                        {
                            Filter = "all",
                            Label = "Все исключения",
                            Description = "Остановка при возникновении любого исключения",
                            SupportsCondition = true,
                            ConditionDescription = "Искомая подстрока текста исключения"
                        }
                    },
                    SupportsEvaluateForHovers = true,
                    SupportTerminateDebuggee = true
                }
            };
        }

        public override Task<SetBreakpointsResponse> SetBreakpointsAsync(SetBreakpointsRequest request, CancellationToken ct)
        {
            if (request.Arguments.SourceModified == true)
            {
                throw new ErrorResponseException("Нельзя установить точку останова на модифицированный файл.");
            }

            Debug.Assert(request.Arguments.Source.Path != null, "request.Arguments.Source.Path != null");
            var path = ToNativePath(request.Arguments.Source.Path);
            
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // vscode иногда передает путь, где диск - маленькая буква
                path = Utilities.NormalizeDriveLetter(path);
            }

            var useConditions = _debuggee.ProtocolVersion >= ProtocolVersions.Version2;
            
            Debug.Assert(request.Arguments.Breakpoints != null, "request.Arguments.Breakpoints != null");
            var breaks = request.Arguments.Breakpoints
                .Select(srcBreakpoint => new OneScript.DebugProtocol.Breakpoint
                {
                    Line = srcBreakpoint.Line,
                    Source = path,
                    Condition = useConditions ? srcBreakpoint.Condition ?? string.Empty : string.Empty
                }).ToList();

            var confirmedBreaks = _debuggee.SetBreakpoints(breaks);
            var confirmedDapBreaks = new List<Breakpoint>(confirmedBreaks.Length);
            confirmedDapBreaks.AddRange(confirmedBreaks
                .Select(t => new Breakpoint
                {
                    Line = t.Line,
                    Verified = true
                })
            );

            return Task.FromResult(new SetBreakpointsResponse
            {
                Body = new SetBreakpointsResponseBody
                {
                    Breakpoints = confirmedDapBreaks.ToArray(),
                }
            });
        }

        public override async Task<LaunchResponse> LaunchAsync(LaunchRequest request, CancellationToken ct)
        {
            try
            {
                Log.LogDebug("Initializing process settings");
                var pathStrategy = new PathHandlingStrategy
                {
                    ClientLinesStartAt1 = Client.LinesStartAt1,
                    ClientPathsAreUri = Client.PathFormat == "uri",
                    DebuggerLinesStartAt1 = true,
                    DebuggerPathsAreUri = false
                };

                _debuggee = DebugeeFactory.CreateProcess(Client.AdapterId, pathStrategy);
                _debuggee.Init(request.Arguments);
            }
            catch (InvalidDebugeeOptionsException e)
            {
                Log.LogError(e, "Wrong options received {ErrorCode}: {Message}", e.ErrorCode, e.Message);
                throw new ErrorResponseException(e.Message);
            }
            
            SubscribeForDebuggeeProcessEvents();
            
            try
            {
                Log.LogDebug("Starting debuggee");
                _debuggee.Start();
                Log.LogInformation("Debuggee started");
            }
            catch (Exception e)
            {
                Log.LogError(e, "Can't launch debuggee");
                throw new ErrorResponseException($"Can't launch debuggee ({e.Message}).");
            }

            DebugClientFactory debugClientFactory;
            try
            {
                debugClientFactory = ConnectDebugServer();
            }
            catch (Exception e)
            {
                _debuggee.Kill();
                await EventsChannel.SendEventAsync(new TerminatedEvent(), ct);
                Log.LogError(e, "Can't connect to debug server");
                throw new ErrorResponseException("Can't connect: " + e.ToString());
            }
            
            _debuggee.SetClient(debugClientFactory.CreateDebugClient());

            return new LaunchResponse();
        }
        
        private void SubscribeForDebuggeeProcessEvents()
        {
            _debuggee.OutputReceived += (s, e) =>
            {
                Log.LogDebug("Output received {Output}", e.Content);

                if (string.IsNullOrEmpty(e.Content)) 
                    return;
                
                var data = e.Content;
                if (data[data.Length - 1] != '\n')
                {
                    data += '\n';
                }

                EventsChannel.SendEventAsync(new OutputEvent
                {
                    Body = new OutputEventBody
                    {
                        Category = e.Category,
                        Output = data
                    }
                });
            };

            _debuggee.ProcessExited += (s, e) =>
            {
                Log.LogInformation("Debuggee has exited");
                EventsChannel.SendEventAsync(new TerminatedEvent());
            };
        }
        
        private DebugClientFactory ConnectDebugServer()
        {
            var tcpConnection = ConnectionFactory.Connect(_debuggee.DebugPort);
            var listener = new OscriptDebugEventsListener(EventsChannel, _threadState);
            return new DebugClientFactory(tcpConnection, listener);
        }
    }
}