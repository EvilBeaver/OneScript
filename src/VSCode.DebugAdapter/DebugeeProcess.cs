/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Diagnostics;
using OneScript.DebugProtocol;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using StackFrame = OneScript.DebugProtocol.StackFrame;

namespace VSCode.DebugAdapter
{
    internal class DebugeeOutputEventArgs : EventArgs
    {
        public DebugeeOutputEventArgs(string category, string content)
        {
            this.Category = category;
            this.Content = content;
        }

        public string Category { get; }
        public string Content { get; }
    }

    internal class DebugeeProcess
    {
        private Process _process;

        private bool _terminated;
        private bool _stdoutEOF;
        private bool _stderrEOF;

        private IDebuggerService _debugger;

        public string RuntimeExecutable { get; set; }
        public string WorkingDirectory { get; set; }
        public string StartupScript { get; set; }
        public string ScriptArguments { get; set; }
        public string RuntimeArguments { get; set; }
        
        public int DebugPort { get; set; }
        
        public string DebugProtocol { get; set; }
        
        

        public bool HasExited => _process.HasExited;
        public int ExitCode => _process.ExitCode;

        private IDebuggerService DebugChannel { get; set; }

        public void Start()
        {
            var exeFileName = Path.GetFileName(RuntimeExecutable);
            if (DebugProtocol == "web")
            {
                RunOsWebRuntime();
            }
            else
            {
                RunOscriptRuntime();
            }
        }

        private void RunOsWebRuntime()
        {
            var dbgArgs = new List<string>();
            if (DebugPort != 0)
            {
                dbgArgs.Add($"--debug.port={DebugPort}");
            }
            dbgArgs.Add("--debug.protocol=tcp");
            dbgArgs.Add("--debug.wait=1");
            
            var debugArguments = string.Join(" ", dbgArgs);
            _process = new Process();
            var psi = _process.StartInfo;
            psi.FileName = RuntimeExecutable;
            psi.UseShellExecute = false;
            psi.Arguments = $"{debugArguments} {RuntimeArguments}";
            psi.WorkingDirectory = WorkingDirectory;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            _process.EnableRaisingEvents = true;
            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.ErrorDataReceived += Process_ErrorDataReceived;
            _process.Exited += Process_Exited;

            SessionLog.WriteLine($"Starting {psi.FileName} with args {psi.Arguments}");
            SessionLog.WriteLine($"cwd = {WorkingDirectory}");

            _process.Start();
            System.Threading.Thread.Sleep(1500);
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        private void RunOscriptRuntime()
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
            _process = new Process();
            var psi = _process.StartInfo;
            psi.FileName = RuntimeExecutable;
            psi.UseShellExecute = false;
            psi.Arguments = $"-debug {debugArguments} {RuntimeArguments} \"{StartupScript}\" {ScriptArguments}";
            psi.WorkingDirectory = WorkingDirectory;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            _process.EnableRaisingEvents = true;
            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.ErrorDataReceived += Process_ErrorDataReceived;
            _process.Exited += Process_Exited;

            SessionLog.WriteLine($"Starting {psi.FileName} with args {psi.Arguments}");
            SessionLog.WriteLine($"cwd = {WorkingDirectory}");

            _process.Start();
            System.Threading.Thread.Sleep(1000);
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public void SetConnection(IDebuggerService service)
        {
            _debugger = service;
        }
        
        public event EventHandler<DebugeeOutputEventArgs> OutputReceived;
        public event EventHandler ProcessExited;
        
        private void Process_Exited(object sender, EventArgs e)
        {
            Terminate();
            ProcessExited?.Invoke(this, new EventArgs());
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                _stdoutEOF = true;
            }
            RaiseOutputReceivedEvent("stdout", e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                _stderrEOF = true;
            }
            RaiseOutputReceivedEvent("stderr", e.Data);
        }

        private void RaiseOutputReceivedEvent(string category, string data)
        {
            OutputReceived?.Invoke(this, new DebugeeOutputEventArgs(category, data));
        }

        private void Terminate()
        {
            if (!_terminated)
            {

                // wait until we've seen the end of stdout and stderr
                for (int i = 0; i < 100 && (_stdoutEOF == false || _stderrEOF == false); i++)
                {
                    System.Threading.Thread.Sleep(100);
                }
                
                _terminated = true;
                _process = null;
                _debugger = null;
            }
        }

        public void Kill()
        {
            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
        }

        public Breakpoint[] SetBreakpoints(IEnumerable<Breakpoint> breakpoints)
        {
            var confirmedBreaks = _debugger.SetMachineBreakpoints(breakpoints.ToArray());
            
            return confirmedBreaks;
        }

        public void BeginExecution(int threadId)
        {
            _debugger.Execute(threadId);
        }
        
        public StackFrame[] GetStackTrace(int threadId, int firstFrameIdx, int limit)
        {
            var allFrames = _debugger.GetStackFrames(threadId);
            
            if (limit == 0)
                limit = allFrames.Length;

            if(allFrames.Length < firstFrameIdx)
                return new StackFrame[0];

            var result = new List<StackFrame>();
            for (int i = firstFrameIdx; i < limit && i < allFrames.Length; i++)
            {
                allFrames[i].ThreadId = threadId;
                result.Add(allFrames[i]);
            }

            return result.ToArray();

        }

        public void FillVariables(IVariableLocator locator)
        {
            locator.Hydrate(_debugger);
        }

        public Variable Evaluate(StackFrame frame, string expression)
        {
            try
            {
                return _debugger.Evaluate(frame.ThreadId, frame.Index, expression);
            }
            catch (FaultException e)
            {
                throw new Exception(e.Message);
            }
        }

        public void Next(int threadId)
        {
            _debugger.Next(threadId);
        }

        public void StepIn(int threadId)
        {
            _debugger.StepIn(threadId);
        }

        internal void StepOut(int threadId)
        {
            _debugger.StepOut(threadId);
        }

        public int[] GetThreads()
        {
            return _debugger.GetThreads();
        }
    }
}
