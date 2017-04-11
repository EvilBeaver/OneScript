using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

using OneScript.DebugProtocol;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;

using StackFrame = OneScript.DebugProtocol.StackFrame;

namespace DebugServer
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
        private TcpClient _client;

        private bool _terminated;
        private bool _stdoutEOF;
        private bool _stderrEOF;

        private IDebuggerService _debugger;

        public string RuntimeExecutable { get; set; }
        public string WorkingDirectory { get; set; }
        public string StartupScript { get; set; }
        public string ScriptArguments { get; set; }
        public string RuntimeArguments { get; set; }

        public bool HasExited => _process.HasExited;
        public int ExitCode => _process.ExitCode;

        public void Start()
        {
            _process = new Process();
            var psi = _process.StartInfo;
            psi.FileName = RuntimeExecutable;
            psi.UseShellExecute = false;
            psi.Arguments = $"-debug {RuntimeArguments} {StartupScript} {ScriptArguments}";
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
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }
        
        public void Connect(int port, IDebugEventListener listener)
        {
            var factory = new DuplexChannelFactory<IDebuggerService>(listener, Binder.GetBinding(), new EndpointAddress(Binder.GetDebuggerUri(port)));
            _debugger = factory.CreateChannel();
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
            SessionLog.WriteLine("sending wcf request BP");
            var confirmedBreaks = _debugger.SetMachineBreakpoints(breakpoints.ToArray());
            
            return confirmedBreaks;
        }

        public void BeginExecution()
        {
            SessionLog.WriteLine("sending wcf request Exec");
            _debugger.Execute();
        }

        internal void ListenToEvents()
        {
            SessionLog.WriteLine("sending wcf request Listen");
            _debugger.RegisterEventListener();
        }

        public StackFrame[] GetStackTrace(int firstFrameIdx, int limit)
        {
            var allFrames = _debugger.GetStackFrames();
            if (limit == 0)
                limit = allFrames.Length;

            SessionLog.WriteLine($"Got params {firstFrameIdx}, lim {limit}");

            if(allFrames.Length < firstFrameIdx)
                return new StackFrame[0];

            var result = new List<StackFrame>();
            for (int i = firstFrameIdx; i < limit && i < allFrames.Length; i++)
            {
                result.Add(allFrames[i]);
            }

            return result.ToArray();

        }

    }
}
