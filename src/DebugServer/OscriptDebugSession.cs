using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using VSCodeDebug;

namespace DebugServer
{
    class OscriptDebugSession : DebugSession
    {
        private DebugeeProcess _process;

        public OscriptDebugSession() : base(true, false)
        {
        }
        
        public override void Initialize(Response response, dynamic args)
        {
            SendResponse(response, new Capabilities()
            {
                supportsConditionalBreakpoints = false,
                supportsFunctionBreakpoints = false,
                supportsConfigurationDoneRequest = false,
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
            
            var process = new DebugeeProcess();
            _process = process;

            process.RuntimeExecutable = runtimeExecutable;
            process.RuntimeArguments = Utilities.ConcatArguments(args.runtimeArgs);
            process.StartupScript = startupScript;
            process.ScriptArguments = Utilities.ConcatArguments(args.args);
            process.WorkingDirectory = workingDirectory;

            process.OutputReceived += (s, e) =>
            {
                SessionLog.WriteLine("output received: " + e.Content);
                SendOutput(e.Category, e.Content);
            };

            process.ProcessExited += (s, e) =>
            {
                SessionLog.WriteLine("process exited");
                SendEvent(new TerminatedEvent());
            };

            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                SendErrorResponse(response, 3012, "Can't launch debugee ({reason}).", new { reason = e.Message });
                return;
            }
            
            var port = getInt(args, "debugPort", 2801);
            try
            {
                process.Connect(port);
            }
            catch (Exception e)
            {
                process.Kill();
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
            RequestDummy("SetBreakpoints request accepted", response, null);
            _process.DebugEventReceived += ProcessOnDebugEventReceived;
            _process.ListenToEvents();
            _process.Send("go");
        }

        private void ProcessOnDebugEventReceived(object sender, DebuggeeEventEventArgs args)
        {
            SessionLog.WriteLine($"got event: {args.Event},\n {args.Body}");
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
            throw new NotImplementedException();
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
            var threads = new List<Thread>();
            threads.Add(new Thread(1, "main"));
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
