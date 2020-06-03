/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VSCode.DebugAdapter
{
    internal class ConsoleProcess : DebugeeProcess
    {
        public ConsoleProcess(PathHandlingStrategy pathHandling) : base(pathHandling)
        {
        }

        public string RuntimeExecutable { get; set; }
        
        public string WorkingDirectory { get; set; }
        
        public string StartupScript { get; set; }
        
        public string ScriptArguments { get; set; }
        
        public string RuntimeArguments { get; set; }

        public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();
        
        protected override void InitInternal(JObject args)
        {
            var options = args.ToObject<ConsoleLaunchOptions>();
            if (options.Program == null)
            {
                throw new InvalidDebugeeOptionsException(1001, "Property 'program' is missing or empty.");
            }

            if (!File.Exists(options.Program))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), options.Program);
                throw new InvalidDebugeeOptionsException(1002, $"Script '{path}' does not exist.");
            }

            // validate argument 'cwd'
            var workingDirectory = options.Cwd;
            if (workingDirectory != null)
            {
                workingDirectory = workingDirectory.Trim();
                if (workingDirectory.Length == 0)
                {
                    throw new InvalidDebugeeOptionsException(3003, "Property 'cwd' is empty.");
                }
                workingDirectory = ConvertClientPathToDebugger(workingDirectory);
                if (!Directory.Exists(workingDirectory))
                {
                    throw new InvalidDebugeeOptionsException(3004, $"Working directory '{workingDirectory}' does not exist.");
                }
            }
            else
            {
                workingDirectory = Path.GetDirectoryName(options.Program);
            }

            WorkingDirectory = workingDirectory;

            // validate argument 'runtimeExecutable'
            var runtimeExecutable = options.RuntimeExecutable;
            if (runtimeExecutable != null)
            {
                runtimeExecutable = runtimeExecutable.Trim();
                if (runtimeExecutable.Length == 0)
                {
                    throw new InvalidDebugeeOptionsException(3005, "Property 'runtimeExecutable' is empty.");
                }

                runtimeExecutable = ConvertClientPathToDebugger(runtimeExecutable);
                if (!File.Exists(runtimeExecutable))
                {
                    throw new InvalidDebugeeOptionsException(3006, $"Runtime executable '{runtimeExecutable}' does not exist.");
                }
            }
            else
            {
                runtimeExecutable = "oscript.exe";
            }

            RuntimeExecutable = runtimeExecutable;
            RuntimeArguments = Utilities.ConcatArguments(options.RuntimeArgs);
            StartupScript = options.Program;
            ScriptArguments = Utilities.ConcatArguments(options.Args);
            DebugProtocol = options.Protocol;
            DebugPort = options.DebugPort;
            Environment = options.Env;
        }

        protected override Process CreateProcess()
        {
            var dbgArgs = new List<string>();
            if (DebugPort != 0)
            {
                dbgArgs.Add($"-port={DebugPort}");
            }
            if (!string.IsNullOrEmpty(DebugProtocol))
            {
                dbgArgs.Add($"-protocol={DebugProtocol}");
            }
            
            var debugArguments = string.Join(" ", dbgArgs);
            var process = new Process();
            var psi = process.StartInfo;
            psi.FileName = RuntimeExecutable;
            psi.UseShellExecute = false;
            psi.Arguments = $"-debug {debugArguments} {RuntimeArguments} \"{StartupScript}\" {ScriptArguments}";
            psi.WorkingDirectory = WorkingDirectory;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            LoadEnvironment(psi, Environment);
            return process;
        }
    }
}