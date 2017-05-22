using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using OneScript.DebugProtocol;
using VSCodeDebug;


namespace DebugServer
{
    internal class OscriptDebugSession : DebugSession, IDebugEventListener
    {
        private DebugeeProcess _process;
        private bool _startupPerformed = false;
        private readonly Handles<OneScript.DebugProtocol.StackFrame> _framesHandles;
        private readonly Handles<IVariableLocator> _variableHandles;

        public OscriptDebugSession() : base(true, false)
        {
            _framesHandles = new Handles<OneScript.DebugProtocol.StackFrame>();
            _variableHandles = new Handles<IVariableLocator>();
        }
        
        public override void Initialize(Response response, dynamic args)
        {
            SendResponse(response, new Capabilities()
            {
                supportsConditionalBreakpoints = false,
                supportsFunctionBreakpoints = false,
                supportsConfigurationDoneRequest = true,
                exceptionBreakpointFilters = new dynamic[0],
                supportsEvaluateForHovers = false
            });

            SendEvent(new InitializedEvent());
        }

        public override void Launch(Response response, dynamic args)
        {
            var startupScript = (string)args["program"];
            if (startupScript == null)
            {
                SendErrorResponse(response, 1001, "Property 'program' is missing or empty.");
                return;
            }

            if (!File.Exists(startupScript) && !Directory.Exists(startupScript))
            {
                SendErrorResponse(response, 1002, "Script '{path}' does not exist.", new { path = startupScript });
                return;
            }

            // validate argument 'args'
            string[] arguments = null;
            if (args.args != null)
            {
                arguments = args.args.ToObject<string[]>();
                if (arguments != null && arguments.Length == 0)
                {
                    arguments = null;
                }
            }

            // validate argument 'cwd'
            var workingDirectory = (string)args.cwd;
            if (workingDirectory != null)
            {
                workingDirectory = workingDirectory.Trim();
                if (workingDirectory.Length == 0)
                {
                    SendErrorResponse(response, 3003, "Property 'cwd' is empty.");
                    return;
                }
                workingDirectory = ConvertClientPathToDebugger(workingDirectory);
                if (!Directory.Exists(workingDirectory))
                {
                    SendErrorResponse(response, 3004, "Working directory '{path}' does not exist.", new { path = workingDirectory });
                    return;
                }
            }
            else
            {
                workingDirectory = Path.GetDirectoryName(startupScript);
            }

            // validate argument 'runtimeExecutable'
            var runtimeExecutable = (string)args.runtimeExecutable;
            if (runtimeExecutable != null)
            {
                runtimeExecutable = runtimeExecutable.Trim();
                if (runtimeExecutable.Length == 0)
                {
                    SendErrorResponse(response, 3005, "Property 'runtimeExecutable' is empty.");
                    return;
                }

                runtimeExecutable = ConvertClientPathToDebugger(runtimeExecutable);
                if (!File.Exists(runtimeExecutable))
                {
                    SendErrorResponse(response, 3006, "Runtime executable '{path}' does not exist.", new
                    {
                        path = runtimeExecutable
                    });
                    return;
                }
            }
            else
            {
                runtimeExecutable = "oscript.exe";
            }
            
            _process = new DebugeeProcess();

            _process.RuntimeExecutable = runtimeExecutable;
            _process.RuntimeArguments = Utilities.ConcatArguments(args.runtimeArgs);
            _process.StartupScript = startupScript;
            _process.ScriptArguments = Utilities.ConcatArguments(args.args);
            _process.WorkingDirectory = workingDirectory;

            _process.OutputReceived += (s, e) =>
            {
                SessionLog.WriteLine("output received: " + e.Content);
                SendOutput(e.Category, e.Content);
            };

            _process.ProcessExited += (s, e) =>
            {
                SessionLog.WriteLine("_process exited");
                SendEvent(new TerminatedEvent());
            };

            try
            {
                _process.Start();
            }
            catch (Exception e)
            {
                SendErrorResponse(response, 3012, "Can't launch debugee ({reason}).", new { reason = e.Message });
                return;
            }
            
            int port = getInt(args, "debugPort", 2801);
            try
            {
                _process.Connect(port, this);
            }
            catch (Exception e)
            {
                _process.Kill();
                _process = null;
                SendErrorResponse(response, 4550, "Can't connect: " + e.ToString());
                return;
            }

            SendResponse(response);

        }
        
        public override void Attach(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Disconnect(Response response, dynamic arguments)
        {
            _process.Kill();
            SendResponse(response);
        }

        public override void SetBreakpoints(Response response, dynamic arguments)
        {
            SessionLog.WriteLine("Set breakpoints command accepted");

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

            var breaks = new List<OneScript.DebugProtocol.Breakpoint>();

            foreach (var srcBreakpoint in arguments.breakpoints)
            {
                var bpt = new OneScript.DebugProtocol.Breakpoint();
                bpt.Line = (int) srcBreakpoint.line;
                bpt.Source = path;
                breaks.Add(bpt);
            }

            SessionLog.WriteLine("process exited: " + _process.HasExited);
            var confirmedBreaks = _process.SetBreakpoints(breaks);
            var confirmedBreaksVSCode = new List<VSCodeDebug.Breakpoint>(confirmedBreaks.Length);
            for (int i = 0; i < confirmedBreaks.Length; i++)
            {
                confirmedBreaksVSCode.Add(new VSCodeDebug.Breakpoint(true, confirmedBreaks[i].Line));
            }

            SendResponse(response, new SetBreakpointsResponseBody(confirmedBreaksVSCode));
            
        }


        public void ThreadStopped(int threadId, ThreadStopReason reason)
        {
            SessionLog.WriteLine("thread stopped");
            _framesHandles.Reset();
            _variableHandles.Reset();
            SendEvent(new StoppedEvent(1, "breakpoint"));
        }

        public override void ConfigurationDone(Response response, dynamic args)
        {
            _process.BeginExecution();
            _startupPerformed = true;
            SendResponse(response);
        }
        
        public override void Continue(Response response, dynamic arguments)
        {
            SendResponse(response);
            _process.BeginExecution();
        }

        public override void Next(Response response, dynamic arguments)
        {
            SendResponse(response);
            lock (_process)
            {
                if (!_process.HasExited)
                {
                    _process.Next();
                }
            }
            
        }

        public override void StepIn(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void StepOut(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Pause(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void StackTrace(Response response, dynamic arguments)
        {
            var firstFrameIdx = (int?)arguments.startFrame ?? 0;
            var limit = (int?) arguments.levels ?? 0;

            var processFrames = _process.GetStackTrace(firstFrameIdx, limit);
            var frames = new VSCodeDebug.StackFrame[processFrames.Length];
            for (int i = 0; i < processFrames.Length; i++)
            {
                frames[i] = new VSCodeDebug.StackFrame(
                    _framesHandles.Create(processFrames[i]),
                    processFrames[i].MethodName,
                    new Source(processFrames[i].Source),
                    processFrames[i].LineNumber, 0);
            }

            SendResponse(response, new StackTraceResponseBody(frames));
        }

        public override void Scopes(Response response, dynamic arguments)
        {
            int frameId = getInt(arguments, "frameId");
            var frame = _framesHandles.Get(frameId, null);
            if (frame == null)
            {
                SendErrorResponse(response, 10001, "No active stackframe");
                return;
            }
            
            var frameVariablesHandle = _variableHandles.Create(frame);
            var scope = new Scope("Локальные переменные", frameVariablesHandle);
            SendResponse(response, new ScopesResponseBody(new Scope[] {scope}));
            SessionLog.WriteLine("Scopes done");
        }

        public override void Variables(Response response, dynamic arguments)
        {
            int varsHandle = getInt(arguments, "variablesReference");
            var variables = _variableHandles.Get(varsHandle, null);
            if (variables == null)
            {
                SendErrorResponse(response, 10001, "No active stackframe");
                return;
            }

            _process.FillVariables(variables);

            var responseArray = new VSCodeDebug.Variable[variables.Count];

            for (int i = 0; i < responseArray.Length; i++)
            {
                var variable = variables[i];

                if (variable.IsStructured && variable.ChildrenHandleID == 0)
                {
                    variable.ChildrenHandleID = _variableHandles.Create(variables.CreateChildLocator(i));
                }

                responseArray[i] = new VSCodeDebug.Variable(
                    variable.Name,
                    variable.Presentation,
                    variable.TypeName,
                    variable.ChildrenHandleID);
            }

            SendResponse(response, new VariablesResponseBody(responseArray));
        }

        public override void Threads(Response response, dynamic arguments)
        {
            var threads = new List<VSCodeDebug.Thread>();
            threads.Add(new VSCodeDebug.Thread(1, "main"));
            SessionLog.WriteLine("Threads request accepted");
            SendResponse(response, new ThreadsResponseBody(threads));
        }

        public override void Evaluate(Response response, dynamic arguments)
        {
            // expression, frameId, context
            int frameId = getInt(arguments, "frameId");
            var frame = _framesHandles.Get(frameId, null);
            if (frame == null)
            {
                SendErrorResponse(response, 10001, "No active stackframe");
                return;
            }

            var expression = (string) arguments.expression;
            OneScript.DebugProtocol.Variable evalResult;
            try
            {
                evalResult = _process.Evaluate(frame, expression);
            }
            catch (Exception e)
            {
                evalResult = new OneScript.DebugProtocol.Variable() { Presentation = e.Message };
            }

            var protResult = new EvaluateResponseBody(evalResult.Presentation) {type = evalResult.TypeName};
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

        private static int getInt(dynamic container, string propertyName, int dflt = 0)
        {
            try
            {
                return (int)container[propertyName];
            }
            catch (Exception)
            {
                // ignore and return default value
            }
            return dflt;
        }


        private void RequestDummy(string message, Response response, dynamic arguments)
        {
            SessionLog.WriteLine(message);
            SendResponse(response, arguments);
        }
    }
}
