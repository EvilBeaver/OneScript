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
using VSCode.DebugAdapter.Transport;
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

        private OneScriptDebuggerClient _debugger;

        private readonly PathHandlingStrategy _strategy;

        private int _activeProtocolVersion;

        private ILogger Log { get; } = Serilog.Log.ForContext<DebugeeProcess>();

        public DebugeeProcess(PathHandlingStrategy pathHandling)
        {
            _strategy = pathHandling;
        }

        public bool HasExited
        {
            get
            {
                if (_process != null)
                    return _process.HasExited;

                if (_attachMode && _debugger != null)
                    return false;
                    
                return true;
            }
        }

        public int ExitCode => _process?.ExitCode ?? 0;

        public int DebugPort { get; set; }

        public int ProtocolVersion
        {
            get => _activeProtocolVersion;
            set
            {
                ValidateProtocolVersion(value);
                _activeProtocolVersion = value;
            }
        }

        public bool WaitOnStart { get; set; }

        public WorkspaceMapper PathsMapper { get; set; }

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

            try
            {
                _process = Process.GetProcessById(pid);
                _process.EnableRaisingEvents = true;
                _process.Exited += Process_Exited;
            }
            catch
            {
                _process = null;
            }

            _attachMode = true;

        }

        public void Init(JObject args)
        {
            InitInternal(args);
        }

        public void InitPathsMapper(JObject args)
        {
            if (args == null)
            {
                PathsMapper = null;
                return;
            }

            try
            {
                var mappingToken = args["pathsMapping"];
                if (mappingToken == null || mappingToken.Type == JTokenType.Null)
                {
                    PathsMapper = null;
                    return;
                }

                PathsMapper = mappingToken.ToObject<WorkspaceMapper>();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to initialize paths mapper; path mapping will be disabled");
                PathsMapper = null;
            }
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

        public void SetClient(OneScriptDebuggerClient service)
        {
            _debugger = service;
            ProtocolVersion = service.ProtocolVersion;
        }

        public event EventHandler<DebugeeOutputEventArgs> OutputReceived;
        public event EventHandler ProcessExited;

        private void Process_Exited(object sender, EventArgs e)
        {
            _debugger?.Stop();
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

        private void ValidateProtocolVersion(int value)
        {
            if (!ProtocolVersions.IsValid(value))
            {
                throw new ArgumentOutOfRangeException($"Protocol version {value} is unknown.");
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
                Log.Debug("Stopping child process...");
                if (_process.WaitForExit(2000))
                {
                    Log.Debug("Process stopped");
                }
                else
                {
                    _process.Kill();
                    Log.Debug("Process killed");
                }
            }

            Log.Debug("Debuggee disconnected");
        }

        public void Kill()
        {
            if (_process == null)
                return;

            _process.Kill();
            _process.WaitForExit(1500);
        }

        public void SetExceptionsBreakpoints((string Id, string Condition)[] filters)
        {
            switch (ProtocolVersion)
            {
                case ProtocolVersions.UnknownVersion:
                case ProtocolVersions.Version1:
                    // Version 1 doesn't support exception breakpoints
                    Log.Warning("Exception breakpoints not supported in protocol version {Version}", ProtocolVersion);
                    break;
                case ProtocolVersions.Version2:
                    _debugger.SetMachineExceptionBreakpoints(filters);
                    break;
                default: // Version 3 and higher
                    _debugger.SetExceptionBreakpoints(filters.Select(t => new ExceptionBreakpointFilter
                    {
                        Id = t.Id,
                        Condition = t.Condition
                    }).ToArray());
                    break;
            }
        }

        public Breakpoint[] SetBreakpoints(IEnumerable<Breakpoint> breakpoints)
        {
            var breakpointsArray = breakpoints.ToArray();

            if (PathsMapper != null)
            {
                for (int i = 0; i < breakpointsArray.Length; i++)
                {
                    breakpointsArray[i].Source = PathsMapper.LocalToRemote(breakpointsArray[i].Source);
                }
            }

            var confirmedBreaks = _debugger.SetMachineBreakpoints(breakpointsArray);

            return confirmedBreaks;
        }

        public void BeginExecution(int threadId)
        {
            _debugger.Execute(threadId);
        }

        public StackFrame[] GetStackTrace(int threadId, int firstFrameIdx, int limit)
        {
            var allFrames = _debugger.GetStackFrames(threadId);
            var pathsMapperInit = PathsMapper != null;

            if (limit == 0)
                limit = allFrames.Length;

            if (allFrames.Length < firstFrameIdx)
                return new StackFrame[0];

            var result = new List<StackFrame>();
            for (int i = firstFrameIdx; i < limit && i < allFrames.Length; i++)
            {

                allFrames[i].ThreadId = threadId;

                if (pathsMapperInit)
                {
                    allFrames[i].Source = PathsMapper.RemoteToLocal(allFrames[i].Source);
                }

                result.Add(allFrames[i]);
            }

            return result.ToArray();

        }

        public Variable[] FetchVariables(IVariablesProvider provider)
        {
            return provider.FetchVariables(_debugger);
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
