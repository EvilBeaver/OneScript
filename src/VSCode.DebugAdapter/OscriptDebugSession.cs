/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using OneScript.DebugProtocol;
using Serilog;
using VSCode.DebugAdapter.Transport;
using VSCodeDebug;


namespace VSCode.DebugAdapter
{
    internal class OscriptDebugSession : DebugSession
    {
        private DebugeeProcess _debuggee;
        private bool _startupPerformed = false;
        private ThreadStateContainer _threadState = new ThreadStateContainer();

        private readonly ILogger Log = Serilog.Log.ForContext<OscriptDebugSession>(); 

        public OscriptDebugSession() : base(true, false)
        {
        }
        
        private string AdapterID { get; set; }
        
        public override void Initialize(Response response, dynamic args)
        {
            LogCommandReceived();
            AdapterID = (string) args.adapterID;

            _debuggee = DebugeeFactory.CreateProcess(AdapterID, PathStrategy);      

            SendResponse(response, new Capabilities
            {
                supportsConditionalBreakpoints = true,
                supportsFunctionBreakpoints = false,
                supportsConfigurationDoneRequest = true,
                supportsExceptionFilterOptions = true,
                exceptionBreakpointFilters = new dynamic[]
                {
                    new
                    {
                        filter = "uncaught",
                        label = "Необработанные исключения",
                        description = "Остановка при возникновении необработанного исключения",
                        supportsCondition = true,
                        conditionDescription = "Искомая подстрока текста исключения"
                    },
                    new
                    {
                        filter = "all",
                        label = "Все исключения",
                        description = "Остановка при возникновении любого исключения",
                        supportsCondition = true,
                        conditionDescription = "Искомая подстрока текста исключения"
                    }
                },
                supportsEvaluateForHovers = true,
                supportTerminateDebuggee = true
            });

            SendEvent(new InitializedEvent());
        }

        public override void Launch(Response response, dynamic args)
        {
            LogCommandReceived();
            try
            {
                Log.Debug("Initializing process settings");
                _debuggee.Init(args);
            }
            catch (InvalidDebugeeOptionsException e)
            {
                Log.Error(e, "Wrong options received {ErrorCode}: {Message}", e.ErrorCode, e.Message);
                SendErrorResponse(response, e.ErrorCode, e.Message);
                return;
            }

            SubscribeForDebuggeeProcessEvents();

            try
            {
                Log.Verbose("Starting debuggee");
                _debuggee.Start();
                Log.Information("Debuggee started");
            }
            catch (Exception e)
            {
                Log.Error(e, "Can't launch debuggee");
                SendErrorResponse(response, 3012, "Can't launch debugee ({reason}).", new { reason = e.Message });
                return;
            }

            DebugClientFactory debugClientFactory;
            try
            {
                debugClientFactory = ConnectDebugServer();
            }
            catch (Exception e)
            {
                _debuggee.Kill();
                SendEvent(new TerminatedEvent());
                Log.Error(e, "Can't connect to debug server");
                SendErrorResponse(response, 4550, "Can't connect: " + e.ToString());
                return;
            }
            
            _debuggee.SetClient(debugClientFactory.CreateDebugClient());

            SendResponse(response);
        }

        private void SubscribeForDebuggeeProcessEvents()
        {
            _debuggee.OutputReceived += (s, e) =>
            {
                Log.Debug("Output received {Output}", e.Content);
                SendOutput(e.Category, e.Content);
            };

            _debuggee.ProcessExited += (s, e) =>
            {
                Log.Information("Debuggee has exited");
                SendEvent(new TerminatedEvent());
            };
        }

        public override void Attach(Response response, dynamic arguments)
        {
            LogCommandReceived();
            SubscribeForDebuggeeProcessEvents();

            _debuggee.DebugPort = GetFromContainer(arguments, "debugPort", 2801);
            _debuggee.InitPathsMapper(arguments);  
            
            DebugClientFactory debugClientFactory;
            try
            {
                debugClientFactory = ConnectDebugServer();
            }
            catch (Exception e)
            {
                Log.Error(e, "Can't connect debuggee");
                SendErrorResponse(response, 4550, "Can't connect: " + e.ToString());
                return;
            }
            
            _debuggee.SetClient(debugClientFactory.CreateDebugClient());
            try
            {
                _debuggee.InitAttached();
            }
            catch (Exception e)
            {
                Log.Error(e, "Attach failed");
                SendErrorResponse(response, 4550, "Attach failed: " + e.ToString());
                return;
            }

            SendResponse(response);
        }

        private DebugClientFactory ConnectDebugServer()
        {
            var tcpConnection = ConnectionFactory.Connect(_debuggee.DebugPort);
            var listener = new OscriptDebugEventsListener(this, _threadState);
            return new DebugClientFactory(tcpConnection, listener);
        }

        public override void Disconnect(Response response, dynamic arguments)
        {
            LogCommandReceived(new { Terminate = arguments.terminateDebuggee });
            bool terminateDebuggee = arguments.terminateDebuggee == true;
            
            _debuggee.HandleDisconnect(terminateDebuggee);
            SendResponse(response);
        }

        public override void SetExceptionBreakpoints(Response response, dynamic arguments)
        {
            LogCommandReceived();
            var acceptedFilters = new List<VSCodeDebug.Breakpoint>();
            var filters = new List<(string Id, string Condition)>();

            foreach(var filter in arguments.filters)
            {
                filters.Add((filter, ""));
                acceptedFilters.Add(new VSCodeDebug.Breakpoint(true));
            }

            foreach (var filterOption in arguments.filterOptions)
            {
                filters.Add((filterOption.filterId, filterOption.condition ?? ""));
                acceptedFilters.Add(new VSCodeDebug.Breakpoint(true));
            }

            _debuggee.SetExceptionsBreakpoints(filters.ToArray());

            SendResponse(response, new SetExceptionBreakpointsResponseBody(acceptedFilters));
        }

        public override void SetBreakpoints(Response response, dynamic arguments)
        {
            LogCommandReceived();
            
            if ((bool)arguments.sourceModified)
            {
                if (_startupPerformed)
                {
                    SendErrorResponse(response, 1102, "Нельзя установить точку останова на модифицированный файл.");
                    return;
                }
                SendResponse(response, new SetBreakpointsResponseBody());
                return;
            }

            var path = (string) arguments.source.path;
            path = ConvertClientPathToDebugger(path);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // vscode иногда передает путь, где диск - маленькая буква
                path = NormalizeDriveLetter(path);
            }
            
            var breaks = new List<OneScript.DebugProtocol.Breakpoint>();

            var useConditions = _debuggee.ProtocolVersion >= ProtocolVersions.Version2;
            
            foreach (var srcBreakpoint in arguments.breakpoints)
            {
                var bpt = new OneScript.DebugProtocol.Breakpoint
                {
                    Line = (int)srcBreakpoint.line,
                    Source = path,
                    Condition = useConditions ? srcBreakpoint.condition ?? string.Empty : string.Empty
                };
                breaks.Add(bpt);
            }

            if(breaks.Count == 0) // в целях сохранения интерфейса WCF придется сделать костыль на перех. период
            {
                var bpt = new OneScript.DebugProtocol.Breakpoint
                {
                    Line = 0,
                    Source = path
                };
                breaks.Add(bpt);
            }
            
            var confirmedBreaks = _debuggee.SetBreakpoints(breaks);
            var confirmedBreaksVSCode = new List<VSCodeDebug.Breakpoint>(confirmedBreaks.Length);
            for (int i = 0; i < confirmedBreaks.Length; i++)
            {
                confirmedBreaksVSCode.Add(new VSCodeDebug.Breakpoint(true, confirmedBreaks[i].Line));
            }
            
            SendResponse(response, new SetBreakpointsResponseBody(confirmedBreaksVSCode));  
        }

        private string NormalizeDriveLetter(string path)
        {
            if (Path.IsPathRooted(path))
                return path[0].ToString().ToUpperInvariant() + path.Substring(1);
            else
                return path;

        }

        public override void ConfigurationDone(Response response, dynamic args)
        {
            if (_debuggee == null)
            {
                Log.Debug("Config Done. Process is not started");
                SendResponse(response);
                return;
            }
            Log.Debug("Config Done. Process is started, sending Execute");
            _debuggee.BeginExecution(-1);
            _startupPerformed = true;
            SendResponse(response);
        }
        
        public override void Continue(Response response, dynamic arguments)
        {
            LogCommandReceived();
            SendResponse(response);
            _debuggee.BeginExecution(-1);
        }

        public override void Next(Response response, dynamic arguments)
        {
            LogCommandReceived();
            SendResponse(response);
            lock (_debuggee)
            {
                if (!_debuggee.HasExited)
                {
                    _debuggee.Next((int)arguments.threadId);
                }
            }
            
        }

        public override void StepIn(Response response, dynamic arguments)
        {
            LogCommandReceived();
            SendResponse(response);
            lock (_debuggee)
            {
                if (!_debuggee.HasExited)
                {
                    _debuggee.StepIn((int)arguments.threadId);
                }
            }
        }

        public override void StepOut(Response response, dynamic arguments)
        {
            LogCommandReceived();
            SendResponse(response);
            lock (_debuggee)
            {
                if (!_debuggee.HasExited)
                {
                    _debuggee.StepOut((int)arguments.threadId);
                }
            }
        }

        public override void Pause(Response response, dynamic arguments)
        {
            LogCommandReceived();
            throw new NotImplementedException();
        }

        public override void StackTrace(Response response, dynamic arguments)
        {
            LogCommandReceived();
            var firstFrameIdx = (int?)arguments.startFrame ?? 0;
            var limit = (int?) arguments.levels ?? 0;
            var threadId = (int) arguments.threadId;
            var processFrames = _debuggee.GetStackTrace(threadId, firstFrameIdx, limit);
            var frames = new VSCodeDebug.StackFrame[processFrames.Length];
            for (int i = 0; i < processFrames.Length; i++)
            {
                frames[i] = new VSCodeDebug.StackFrame(
                    _threadState.RegisterFrame(processFrames[i]),
                    processFrames[i].MethodName,
                    processFrames[i].GetSource(),
                    processFrames[i].LineNumber, 0);
            }

            SendResponse(response, new StackTraceResponseBody(frames));
        }

        public override void Scopes(Response response, dynamic arguments)
        {
            LogCommandReceived();
            int frameId = GetFromContainer(arguments, "frameId", 0);
            var frame = _threadState.GetFrameById(frameId);
            if (frame == null)
            {
                SendErrorResponse(response, 10001, "No active stackframe");
                return;
            }

            var scopes = new List<Scope>();
            
            // Scope 1: Локальные переменные
            var localProvider = new LocalScopeProvider(frame.ThreadId, frame.Index);
            var localHandle = _threadState.RegisterVariablesProvider(localProvider);
            scopes.Add(new Scope("Локальные переменные", localHandle));
            
            // Scope 2: Переменные модуля (начиная с протокола версии 4)
            if (_debuggee.ProtocolVersion >= ProtocolVersions.Version4)
            {
                var moduleProvider = new ModuleScopeProvider(frame.ThreadId, frame.Index);
                var moduleHandle = _threadState.RegisterVariablesProvider(moduleProvider);
                scopes.Add(new Scope("Переменные модуля", moduleHandle));
            }
            
            SendResponse(response, new ScopesResponseBody(scopes.ToArray()));
        }

        public override void Variables(Response response, dynamic arguments)
        {
            LogCommandReceived();
            int varsHandle = GetFromContainer(arguments, "variablesReference", 0);
            var provider = _threadState.GetVariablesProviderById(varsHandle);
            if (provider == null)
            {
                SendErrorResponse(response, 10001, "Invalid variables reference");
                return;
            }

            // Получаем переменные через провайдер
            var variables = _debuggee.FetchVariables(provider);
            var responseArray = new VSCodeDebug.Variable[variables.Length];

            for (int i = 0; i < responseArray.Length; i++)
            {
                var variable = variables[i];
                int childHandle = 0;

                if (variable.IsStructured)
                {
                    var childProvider = provider.CreateChildProvider(i);
                    childHandle = _threadState.RegisterVariablesProvider(childProvider);
                }

                responseArray[i] = new VSCodeDebug.Variable(
                    variable.Name,
                    variable.Presentation,
                    variable.TypeName,
                    childHandle);
            }

            SendResponse(response, new VariablesResponseBody(responseArray));
        }

        public override void Threads(Response response, dynamic arguments)
        {
            LogCommandReceived();
            var threads = new List<VSCodeDebug.Thread>();
            var processThreads = _debuggee.GetThreads();
            for (int i = 0; i < processThreads.Length; i++)
            {
                threads.Add(new VSCodeDebug.Thread(processThreads[i], $"Thread {processThreads[i]}"));
            }
            
            SendResponse(response, new ThreadsResponseBody(threads));
        }

        public override void Evaluate(Response response, dynamic arguments)
        {
            LogCommandReceived();
            // expression, frameId, context
            int frameId = GetFromContainer(arguments, "frameId", 0);
            var frame = _threadState.GetFrameById(frameId);
            if (frame == null)
            {
                SendErrorResponse(response, 10001, "No active stackframe");
                return;
            }

            var expression = (string) arguments.expression;
            var context = (string) arguments.context;
            
            Log.Debug("Evaluate {Expression} in {Context}", expression, context);
             
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

            if (evalResult.Name.Equals("$evalFault") && context.Equals("hover"))
            {
                evalResult.Presentation = $"err: {expression}";
            }

            var protResult = new EvaluateResponseBody(evalResult.Presentation, id) {type = evalResult.TypeName};
            SendResponse(response, protResult);
        }


        private void SendOutput(string category, string data)
        {
            if (!String.IsNullOrEmpty(data))
            {
                if (data[data.Length - 1] != '\n')
                {
                    data += '\n';
                }
                SendEvent(new OutputEvent(category, data));
            }
        }

        private static T GetFromContainer<T>(dynamic container, string propertyName, T defaultValue = default)
        {
            try
            {
                return (T)container[propertyName];
            }
            catch (Exception)
            {
                // ignore and return default value
            }
            return defaultValue;
        }

        protected override void OnRequestError(Exception e)
        {
            Log.Error(e, "Unhandled request processing error");
        }

        private void LogCommandReceived(dynamic args = null, [CallerMemberName] string commandName = "")
        {
            if (args == null)
                Log.Debug("Command received {Command}", commandName);
            else
                Log.Debug("Command received {Command}: {@Args}", commandName, args);
        }
    }
}
