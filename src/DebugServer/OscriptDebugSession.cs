using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using OneScript.DebugProtocol;
using VSCodeDebug;

namespace DebugServer
{
    class OscriptDebugSession : DebugSession
    {
        private DebugeeProcess _process;
        private bool _startupPerformed = false;

        public OscriptDebugSession() : base(true, false)
        {
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
            
            var port = getInt(args, "debugPort", 2801);
            try
            {
                _process.Connect(port);
            }
            catch (Exception e)
            {
                _process.Kill();
                SendErrorResponse(response, 4550, "Process socket doesn't respond: " + e.ToString());
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

            var confirmedBreaks = _process.SetBreakpoints(breaks);
            var confirmedBreaksVSCode = new List<VSCodeDebug.Breakpoint>(confirmedBreaks.Length);
            for (int i = 0; i < confirmedBreaks.Length; i++)
            {
                confirmedBreaksVSCode.Add(new VSCodeDebug.Breakpoint(true, confirmedBreaks[i].Line));
            }

            SendResponse(response, new SetBreakpointsResponseBody(confirmedBreaksVSCode));
            
        }

        public override void ConfigurationDone(Response response, dynamic args)
        {
            _process.ListenToEvents(DebuggeeEventListener);
            _process.BeginExecution();
            _startupPerformed = true;
            SendResponse(response);
        }

        private void DebuggeeEventListener(DebugEventListener listener, DebugProtocolMessage eventData)
        {
            SessionLog.WriteLine("Debuggee event: " + eventData.ToSerializedString());
            switch (eventData.Name)
            {
                case "Breakpoint":
                    SendEvent(new StoppedEvent((int)eventData.Data, "breakpoint hit"));
                    break;
            }
        }

        public override void Continue(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Next(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
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

            var frames = _process.GetStackTrace(firstFrameIdx, limit);

            RequestDummy("stacktrace dummy", response, new StackTraceResponseBody(new List<VSCodeDebug.StackFrame>()));
        }

        public override void Scopes(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Variables(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Threads(Response response, dynamic arguments)
        {
            var threads = new List<VSCodeDebug.Thread>();
            threads.Add(new VSCodeDebug.Thread(1, "main"));
            SessionLog.WriteLine("Threads request accepted: " + _process?.HasExited);
            SendResponse(response, new ThreadsResponseBody(threads));
        }

        public override void Evaluate(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
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
