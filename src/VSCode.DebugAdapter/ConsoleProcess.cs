/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json.Linq;
using Serilog;
using VSCode.DebugAdapter.Transport;

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
        
        public bool RunInTerminal { get; set; }
        
        protected override void InitInternal(JObject args)
        {
            var options = args.ToObject<ConsoleLaunchOptions>();
            if (options.Program == null)
            {
                throw new InvalidDebugeeOptionsException(1001, "Property 'program' is missing or empty.");
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
            
            // Кодировка DAP
            SetEncoding(options.OutputEncoding);
            
            WorkingDirectory = workingDirectory;
            Log.Information("Working directory for debuggee is {WorkingDirectory}", WorkingDirectory);
            
            var programPath = Path.Combine(workingDirectory ?? Directory.GetCurrentDirectory(), options.Program);
            if (!File.Exists(programPath))
            {
                throw new InvalidDebugeeOptionsException(1002, $"Script '{programPath}' does not exist.");
            }

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
            DebugPort = options.DebugPort;
            Environment = options.Env;
            WaitOnStart = options.WaitOnStart ?? true;
            RunInTerminal = options.RunInTerminal ?? false;
        }

        protected override Process CreateProcess()
        {
            var dbgArgs = new List<string>();
            if (DebugPort != 0)
            {
                dbgArgs.Add($"-port={DebugPort}");
            }

            if (!WaitOnStart)
            {
                dbgArgs.Add("-noWait");
            }
            
            var debugArguments = string.Join(" ", dbgArgs);
            var commandLine = $"{RuntimeArguments} -debug {debugArguments} \"{StartupScript}\" {ScriptArguments}";
            
            var process = new Process();
            var psi = process.StartInfo;
            
            if (RunInTerminal)
            {
                // Запуск в отдельном окне терминала
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    ConfigureWindowsTerminalLaunch(psi, commandLine);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    ConfigureMacOSTerminalLaunch(psi, commandLine);
                }
                else
                {
                    ConfigureLinuxTerminalLaunch(psi, commandLine);
                }
            }
            else
            {
                ConfigureNormalLaunch(psi, commandLine);
            }
            
            psi.WorkingDirectory = WorkingDirectory;
            // В режиме терминала переменные окружения уже установлены в batch-файле (Windows) или команде (Linux/Mac)
            // В обычном режиме загружаем переменные окружения
            if (!RunInTerminal)
            {
                LoadEnvironment(psi, Environment);
            }
            return process;
        }
        
        private void ConfigureWindowsTerminalLaunch(ProcessStartInfo psi, string commandLine)
        {
            // Windows: создаем временный batch-файл для запуска в новом окне терминала
            // Это позволяет корректно передать переменные окружения
            var tempBatchFile = Path.Combine(Path.GetTempPath(), $"onescript_debug_{System.Guid.NewGuid():N}.bat");
            CreateWindowsBatchFile(tempBatchFile, commandLine);
            
            psi.FileName = "cmd.exe";
            psi.Arguments = $"/c start \"OneScript Debug\" cmd.exe /k \"{tempBatchFile}\"";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = false;
            psi.WindowStyle = ProcessWindowStyle.Normal;
            
            // Удаляем временный файл после запуска процесса
            ScheduleBatchFileCleanup(tempBatchFile);
        }
        
        private void CreateWindowsBatchFile(string batchFilePath, string commandLine)
        {
            using (var writer = new StreamWriter(batchFilePath, false, Encoding.Default))
            {
                writer.WriteLine("@echo off");
                writer.WriteLine($"cd /d \"{WorkingDirectory}\"");
                // Устанавливаем переменные окружения
                foreach (var envVar in Environment)
                {
                    writer.WriteLine($"set \"{envVar.Key}={envVar.Value}\"");
                }
                writer.WriteLine($"\"{RuntimeExecutable}\" {commandLine}");
                writer.WriteLine("if errorlevel 1 pause");
            }
        }
        
        private void ScheduleBatchFileCleanup(string batchFilePath)
        {
            System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
            {
                try
                {
                    if (File.Exists(batchFilePath))
                        File.Delete(batchFilePath);
                }
                catch
                {
                    // Ignore
                }
            });
        }
        
        private void ConfigureMacOSTerminalLaunch(ProcessStartInfo psi, string commandLine)
        {
            // macOS: используем osascript для запуска в Terminal.app
            var escapedCommandLine = commandLine.Replace("\"", "\\\"");
            var script = $"tell application \"Terminal\" to do script \"cd '{WorkingDirectory}' && {RuntimeExecutable} {escapedCommandLine}\"";
            
            psi.FileName = "osascript";
            psi.Arguments = $"-e \"{script}\"";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = false;
        }
        
        private void ConfigureLinuxTerminalLaunch(ProcessStartInfo psi, string commandLine)
        {
            // Linux: пробуем различные терминалы
            var terminal = FindTerminal();
            if (terminal != null)
            {
                var terminalArguments = BuildLinuxTerminalCommand(terminal, commandLine);
                psi.FileName = terminal;
                psi.Arguments = terminalArguments;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = false;
            }
            else
            {
                // Fallback: обычный запуск
                Log.Warning("Terminal not found, falling back to normal process launch");
                ConfigureNormalLaunch(psi, commandLine);
            }
        }
        
        private string BuildLinuxTerminalCommand(string terminal, string commandLine)
        {
            // Формируем команду с переменными окружения
            var envPrefix = BuildEnvironmentPrefix();
            var escapedCommandLine = commandLine.Replace("\"", "\\\"").Replace("$", "\\$");
            var bashCommand = $"{envPrefix}cd '{WorkingDirectory}' && {RuntimeExecutable} {escapedCommandLine}; exec bash";
            
            if (terminal.Contains("gnome-terminal") || terminal.Contains("tilix"))
            {
                return $"--working-directory=\"{WorkingDirectory}\" -- bash -c \"{bashCommand}\"";
            }
            else if (terminal.Contains("xterm"))
            {
                return $"-e bash -c \"{bashCommand}\"";
            }
            else
            {
                // Общий формат для других терминалов
                return $"-e bash -c \"{bashCommand}\"";
            }
        }
        
        private string BuildEnvironmentPrefix()
        {
            if (Environment == null || Environment.Count == 0)
                return string.Empty;
            
            var envVars = new StringBuilder();
            foreach (var envVar in Environment)
            {
                var escapedValue = envVar.Value.Replace("'", "'\"'\"'");
                envVars.Append($"{envVar.Key}='{escapedValue}' ");
            }
            return envVars.ToString();
        }
        
        private void ConfigureNormalLaunch(ProcessStartInfo psi, string commandLine)
        {
            // Обычный режим: перенаправляем потоки
            psi.FileName = RuntimeExecutable;
            psi.Arguments = commandLine;
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
        }
        
        private string FindTerminal()
        {
            var terminals = new[] { "gnome-terminal", "xterm", "konsole", "tilix", "terminator" };
            foreach (var terminal in terminals)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "which",
                        Arguments = terminal,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                try
                {
                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                    {
                        return output.Trim();
                    }
                }
                catch
                {
                    // Ignore
                }
            }
            return null;
        }
    }
}