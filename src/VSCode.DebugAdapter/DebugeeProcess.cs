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
using System.Linq;
using Newtonsoft.Json.Linq;
using StackFrame = OneScript.DebugProtocol.StackFrame;

namespace VSCode.DebugAdapter
{
    internal abstract class DebugeeProcess
    {
        private Process _process;

        private bool _terminated;
        private bool _stdoutEOF;
        private bool _stderrEOF;

        private IDebuggerService _debugger;

        private readonly PathHandlingStrategy _strategy;

        public DebugeeProcess(PathHandlingStrategy pathHandling)
        {
            _strategy = pathHandling;
        }
        
        public string DebugProtocol { get; protected set; }
        
        public bool HasExited => _process?.HasExited ?? true;
        public int ExitCode => _process.ExitCode;

        private IDebuggerService DebugChannel { get; set; }
        public int DebugPort { get; set; }

        public void Start()
        {
            _process = CreateProcess();
            var psi = _process.StartInfo;
            
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            _process.EnableRaisingEvents = true;
            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.ErrorDataReceived += Process_ErrorDataReceived;
            _process.Exited += Process_Exited;
            
            _process.Start();
            System.Threading.Thread.Sleep(1500);
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public void Init(JObject args)
        {
            InitInternal(args);
        }
        
        protected abstract Process CreateProcess();

        protected abstract void InitInternal(JObject args);

        protected string ConvertClientPathToDebugger(string clientPath)
        {
            return _strategy.ConvertClientPathToDebugger(clientPath);
        }
        
        protected void LoadEnvironment(ProcessStartInfo psi, IDictionary<string, string> variables)
        {
            if (variables == null || variables.Count <= 0)
                return;
            
            foreach (var pair in variables)
            {
                psi.EnvironmentVariables[pair.Key] = pair.Value;
            }
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
            catch (RpcOperationException e)
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

        public void InitAttached()
        {
            var pid = _debugger.GetProcessId();
            _process = Process.GetProcessById(pid);
            _process.EnableRaisingEvents = true;
            _process.Exited += Process_Exited;
        }
    }
}
