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

namespace VSCode.DebugAdapter.OscriptProtocols
{
    internal class ServerProcess : DebugeeProcess
    {
        public ServerProcess(PathHandlingStrategy pathHandling) : base(pathHandling)
        {
        }

        public string RuntimeExecutable { get; set; }
        
        public string RuntimeArguments { get; set; }
        
        public string WorkingDirectory { get; set; }
        
        public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();
        
        protected override Process CreateProcess()
        {
            var dbgArgs = new List<string>();
            if (DebugPort != 0)
            {
                dbgArgs.Add($"--debug.port={DebugPort}");
            }
            dbgArgs.Add("--debug.protocol=tcp");
            dbgArgs.Add("--debug.wait=1");
            
            var debugArguments = string.Join(" ", dbgArgs);
            var process = new Process();
            var psi = process.StartInfo;
            psi.FileName = RuntimeExecutable;
            psi.UseShellExecute = false;
            psi.Arguments = $"{debugArguments} {RuntimeArguments}";
            psi.WorkingDirectory = WorkingDirectory;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            LoadEnvironment(psi, Environment);

            return process;
        }

        protected override void InitInternal(JObject args)
        {
            var options = args.ToObject<WebLaunchOptions>();
            
            // validate argument 'cwd'
            var workingDirectory = options.AppDir;
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
                throw new InvalidDebugeeOptionsException(3004, "Application directory 'appDir' is not specified.");
            }

            options.AppDir = workingDirectory;

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
                throw new InvalidDebugeeOptionsException(3004, "Runtime executable 'runtimeExecutable' is not specified.");
            }

            RuntimeExecutable = runtimeExecutable;
            RuntimeArguments = Utilities.ConcatArguments(options.RuntimeArgs);
            WorkingDirectory = options.AppDir;
            DebugPort = options.DebugPort;
            Environment = options.Env;
            DebugProtocol = "tcp";
        }
    }
}