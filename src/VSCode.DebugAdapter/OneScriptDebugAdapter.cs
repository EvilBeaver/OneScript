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
        private readonly ThreadStateContainer _threadState = new ThreadStateContainer();
        
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

        public override Task<ConfigurationDoneResponse> ConfigurationDoneAsync(ConfigurationDoneRequest request, CancellationToken ct)
        {
            if (_debuggee == null)
            {
                Log.LogDebug("Config Done. Process is not started");
                return Task.FromResult(new ConfigurationDoneResponse());
            }

            Log.LogDebug("Config Done. Process is started, sending Execute");
            _debuggee.BeginExecution(-1);

            return Task.FromResult(new ConfigurationDoneResponse());
        }

        public override Task<DisconnectResponse> DisconnectAsync(DisconnectRequest request, CancellationToken ct)
        {
            Log.LogDebug("Disconnect requested, terminate={Terminate}", request.Arguments?.TerminateDebuggee);
            bool terminateDebuggee = request.Arguments?.TerminateDebuggee == true;

            _debuggee?.HandleDisconnect(terminateDebuggee);

            return Task.FromResult(new DisconnectResponse());
        }

        public override Task<ContinueResponse> ContinueAsync(ContinueRequest request, CancellationToken ct)
        {
            _debuggee.BeginExecution(-1);
            return Task.FromResult(new ContinueResponse());
        }

        public override Task<NextResponse> NextAsync(NextRequest request, CancellationToken ct)
        {
            lock (_debuggee)
            {
                if (!_debuggee.HasExited)
                    _debuggee.Next(request.Arguments.ThreadId);
            }
            return Task.FromResult(new NextResponse());
        }

        public override Task<StepInResponse> StepInAsync(StepInRequest request, CancellationToken ct)
        {
            lock (_debuggee)
            {
                if (!_debuggee.HasExited)
                    _debuggee.StepIn(request.Arguments.ThreadId);
            }
            return Task.FromResult(new StepInResponse());
        }

        public override Task<StepOutResponse> StepOutAsync(StepOutRequest request, CancellationToken ct)
        {
            lock (_debuggee)
            {
                if (!_debuggee.HasExited)
                    _debuggee.StepOut(request.Arguments.ThreadId);
            }
            return Task.FromResult(new StepOutResponse());
        }

        public override Task<ThreadsResponse> ThreadsAsync(ThreadsRequest request, CancellationToken ct)
        {
            var processThreads = _debuggee.GetThreads();
            var threads = new EvilBeaver.DAP.Dto.Types.Thread[processThreads.Length];
            for (int i = 0; i < processThreads.Length; i++)
            {
                threads[i] = new EvilBeaver.DAP.Dto.Types.Thread
                {
                    Id = processThreads[i],
                    Name = $"Thread {processThreads[i]}"
                };
            }

            return Task.FromResult(new ThreadsResponse
            {
                Body = new ThreadsResponseBody { Threads = threads }
            });
        }

        public override Task<StackTraceResponse> StackTraceAsync(StackTraceRequest request, CancellationToken ct)
        {
            var args = request.Arguments;
            var firstFrameIdx = args.StartFrame ?? 0;
            var limit = args.Levels ?? 0;
            var threadId = args.ThreadId;

            var processFrames = _debuggee.GetStackTrace(threadId, firstFrameIdx, limit);
            var frames = new EvilBeaver.DAP.Dto.Types.StackFrame[processFrames.Length];
            for (int i = 0; i < processFrames.Length; i++)
            {
                frames[i] = new EvilBeaver.DAP.Dto.Types.StackFrame
                {
                    Id = _threadState.RegisterFrame(processFrames[i]),
                    Name = processFrames[i].MethodName,
                    Source = processFrames[i].GetSource(),
                    Line = processFrames[i].LineNumber,
                    Column = 0
                };
            }

            return Task.FromResult(new StackTraceResponse
            {
                Body = new StackTraceResponseBody
                {
                    StackFrames = frames,
                    TotalFrames = frames.Length
                }
            });
        }

        public override Task<ScopesResponse> ScopesAsync(ScopesRequest request, CancellationToken ct)
        {
            int frameId = request.Arguments.FrameId;
            var frame = _threadState.GetFrameById(frameId);
            if (frame == null)
            {
                throw new ErrorResponseException("No active stackframe");
            }

            var scopes = new List<Scope>();

            var localProvider = new LocalScopeProvider(frame.ThreadId, frame.Index);
            var localHandle = _threadState.RegisterVariablesProvider(localProvider);
            scopes.Add(new Scope
            {
                Name = "Локальные переменные",
                VariablesReference = localHandle
            });

            if (_debuggee.ProtocolVersion >= ProtocolVersions.Version4)
            {
                var moduleProvider = new ModuleScopeProvider(frame.ThreadId, frame.Index);
                var moduleHandle = _threadState.RegisterVariablesProvider(moduleProvider);
                scopes.Add(new Scope
                {
                    Name = "Переменные модуля",
                    VariablesReference = moduleHandle
                });
            }

            return Task.FromResult(new ScopesResponse
            {
                Body = new ScopesResponseBody { Scopes = scopes.ToArray() }
            });
        }

        public override Task<VariablesResponse> VariablesAsync(VariablesRequest request, CancellationToken ct)
        {
            int varsHandle = request.Arguments.VariablesReference;
            var provider = _threadState.GetVariablesProviderById(varsHandle);
            if (provider == null)
            {
                throw new ErrorResponseException("Invalid variables reference");
            }

            var variables = _debuggee.FetchVariables(provider);
            var responseArray = new Variable[variables.Length];

            for (int i = 0; i < responseArray.Length; i++)
            {
                var variable = variables[i];
                int childHandle = 0;

                if (variable.IsStructured)
                {
                    var childProvider = provider.CreateChildProvider(i);
                    childHandle = _threadState.RegisterVariablesProvider(childProvider);
                }

                responseArray[i] = new Variable
                {
                    Name = variable.Name,
                    Value = variable.Presentation,
                    Type = variable.TypeName,
                    VariablesReference = childHandle
                };
            }

            return Task.FromResult(new VariablesResponse
            {
                Body = new VariablesResponseBody { Variables = responseArray }
            });
        }

        public override Task<EvaluateResponse> EvaluateAsync(EvaluateRequest request, CancellationToken ct)
        {
            var args = request.Arguments;
            int frameId = args.FrameId ?? 0;
            var frame = _threadState.GetFrameById(frameId);
            if (frame == null)
            {
                throw new ErrorResponseException("No active stackframe");
            }

            var expression = args.Expression;
            var context = args.Context;

            int id = 0;
            OneScript.DebugProtocol.Variable evalResult;
            try
            {
                evalResult = _debuggee.Evaluate(frame, expression);
                if (evalResult.IsStructured)
                {
                    var provider = new EvaluatedExpressionProvider(expression, frame.ThreadId, frame.Index);
                    id = _threadState.RegisterVariablesProvider(provider);
                }
            }
            catch (Exception e)
            {
                evalResult = new OneScript.DebugProtocol.Variable() { Presentation = e.Message, Name = "$evalFault" };
            }

            if (evalResult.Name.Equals("$evalFault") && "hover".Equals(context))
            {
                evalResult.Presentation = $"err: {expression}";
            }

            return Task.FromResult(new EvaluateResponse
            {
                Body = new EvaluateResponseBody
                {
                    Result = evalResult.Presentation,
                    Type = evalResult.TypeName,
                    VariablesReference = id
                }
            });
        }

        public override Task<SetExceptionBreakpointsResponse> SetExceptionBreakpointsAsync(SetExceptionBreakpointsRequest request, CancellationToken ct)
        {
            var args = request.Arguments;
            var filters = new List<(string Id, string Condition)>();
            var acceptedFilters = new List<Breakpoint>();

            if (args.Filters != null)
            {
                foreach (var filter in args.Filters)
                {
                    filters.Add((filter, ""));
                    acceptedFilters.Add(new Breakpoint { Verified = true });
                }
            }

            if (args.FilterOptions != null)
            {
                foreach (var filterOption in args.FilterOptions)
                {
                    filters.Add((filterOption.FilterId, filterOption.Condition ?? ""));
                    acceptedFilters.Add(new Breakpoint { Verified = true });
                }
            }

            _debuggee.SetExceptionsBreakpoints(filters.ToArray());

            return Task.FromResult(new SetExceptionBreakpointsResponse
            {
                Body = new SetExceptionBreakpointsResponseBody
                {
                    Breakpoints = acceptedFilters.ToArray()
                }
            });
        }

        public override Task<AttachResponse> AttachAsync(AttachRequest request, CancellationToken ct)
        {
            var options = request.Arguments.DeserializeAdditionalProperties<AttachOptions>();

            var pathStrategy = new PathHandlingStrategy
            {
                ClientLinesStartAt1 = Client.LinesStartAt1,
                ClientPathsAreUri = Client.PathFormat == "uri",
                DebuggerLinesStartAt1 = true,
                DebuggerPathsAreUri = false
            };

            _debuggee = DebugeeFactory.CreateAttachableProcess(Client.AdapterId, pathStrategy);
            _debuggee.DebugPort = options.DebugPort;
            _debuggee.PathsMapper = options.PathsMapping;

            SubscribeForDebuggeeProcessEvents();

            DebugClientFactory debugClientFactory;
            try
            {
                debugClientFactory = ConnectDebugServer();
            }
            catch (Exception e)
            {
                Log.LogError(e, "Can't connect debuggee");
                throw new ErrorResponseException("Can't connect: " + e.ToString());
            }

            _debuggee.SetClient(debugClientFactory.CreateDebugClient());
            try
            {
                _debuggee.InitAttached();
            }
            catch (Exception e)
            {
                Log.LogError(e, "Attach failed");
                throw new ErrorResponseException("Attach failed: " + e.ToString());
            }

            return Task.FromResult(new AttachResponse());
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