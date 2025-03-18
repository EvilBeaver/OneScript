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
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json.Linq;
using Serilog;
using VSCode.DebugAdapter.OscriptProtocols;
using StackFrame = OneScript.DebugProtocol.StackFrame;

namespace VSCode.DebugAdapter
{
    internal abstract class DebugeeProcess
    {
        private Process _process;

        private bool _terminated;
        private bool _stdoutEOF;
        private bool _stderrEOF;
        private bool _attachMode;
        
        private Encoding _dapEncoding;

        private TcpDebugServerClient _debugger;

        private readonly PathHandlingStrategy _strategy;

        private int _activeProtocolVersion;

        public DebugeeProcess(PathHandlingStrategy pathHandling)
        {
            _strategy = pathHandling;
        }
        
        public string DebugProtocol { get; protected set; }
        
        public bool HasExited => _process?.HasExited ?? true;
        public int ExitCode => _process.ExitCode;

        public int DebugPort { get; set; }

        public int ProtocolVersion
        {
            get => _activeProtocolVersion;
            set
            {
                _activeProtocolVersion = value;
                SetupSupportedProtocolVersion();
            }
        }

        public void Start()
        {
            _process = CreateProcess();
            var psi = _process.StartInfo;
            
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            if (_dapEncoding != null)
            {
                psi.StandardErrorEncoding = _dapEncoding;
                psi.StandardOutputEncoding = _dapEncoding;
            }

            _process.EnableRaisingEvents = true;
            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.ErrorDataReceived += Process_ErrorDataReceived;
            _process.Exited += Process_Exited;
            _attachMode = false;
            _process.Start();
            System.Threading.Thread.Sleep(1500);
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }
        
        public void InitAttached()
        {
            var pid = _debugger.GetProcessId();
            _process = Process.GetProcessById(pid);
            _attachMode = true;
            _process.EnableRaisingEvents = true;
            _process.Exited += Process_Exited;
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

        protected void SetEncoding(string encodingFromOptions)
        {
            if (string.IsNullOrWhiteSpace(encodingFromOptions))
            {
                _dapEncoding = DefaultEncoding();
            }
            else
            {
                _dapEncoding = Utilities.GetEncodingFromOptions(encodingFromOptions);
            }
            
            Log.Information("Encoding for debuggee output is {Encoding}", _dapEncoding);
        }

        private Encoding DefaultEncoding()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? null : Encoding.UTF8;
        }

        public void SetConnection(TcpDebugServerClient service)
        {
            _debugger = service;
        }
        
        public event EventHandler<DebugeeOutputEventArgs> OutputReceived;
        public event EventHandler ProcessExited;
        
        private void Process_Exited(object sender, EventArgs e)
        {
            _debugger?.Disconnect();
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

        private void SetupSupportedProtocolVersion()
        {
            if (!ProtocolVersions.IsValid(_activeProtocolVersion))
            {
                _activeProtocolVersion = ProtocolVersions.SafestVersion;
                return;
            }

            if (_activeProtocolVersion != ProtocolVersions.UnknownVersion)
            {
                // Задали вручную корректное значение. Ничего не запрашиваем
                return;
            }

            try
            {
                _activeProtocolVersion = _debugger.GetProtocolVersion();
            }
            catch (Exception e)
            {
                Log.Error(e, "Unknown error while checking version");
            }
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

        public void HandleDisconnect(bool terminate)
        {
            if (_debugger == null)
            {
                Log.Debug("Debugger is not connected. Nothing to disconnect");
                return;
            }
            _debugger.Disconnect(terminate);

            var mustKill = terminate && !_attachMode;
            
            if (mustKill && _process != null && !_process.HasExited)
            {
                if (!_process.WaitForExit(2000))
                    _process.Kill();
            }
        }

        public void Kill()
        {
            _process.Kill();
            _process.WaitForExit(1500);
        }

        public void SetExceptionsBreakpoints((string Id, string Condition)[] filters)
        {
            if (ProtocolVersion > ProtocolVersions.Version1)
                _debugger.SetMachineExceptionBreakpoints(filters);
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
    }
}
