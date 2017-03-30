using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

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

    internal class DebuggeeEventEventArgs : EventArgs
    {
        public string Event { get; set; }
        public string Body { get; set; }
    }

    internal class DebugeeProcess
    {

        private Process _process;
        private TcpClient _client;

        private bool _terminated;
        private bool _stdoutEOF;
        private bool _stderrEOF;

        private DebugEventListener _listener;

        private EventHandler<DebuggeeEventEventArgs> _dbgEventHandler;

        public string RuntimeExecutable { get; set; }
        public string WorkingDirectory { get; set; }
        public string StartupScript { get; set; }
        public string ScriptArguments { get; set; }
        public string RuntimeArguments { get; set; }

        public bool HasExited => _process.HasExited;
        public int ExitCode => _process.ExitCode;

        public DebugeeProcess()
        {
            // null handler
            _dbgEventHandler += (s1, s2) => { };
        }

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
        
        public void Connect(int port)
        {
            _client = new TcpClient("localhost", port);
        }

        public void Send(string something)
        {
            SessionLog.WriteLine("Sending " + something);
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms, Encoding.UTF8, true);
            writer.Write(something);
            writer.Dispose();

            var s = _client.GetStream();
            var bytes = ms.GetBuffer();
            s.Write(bytes, 0, (int)ms.Length);
            ms.Dispose();
        }

        private void OnDebugEventReceived(string category, string body)
        {
            SessionLog.WriteLine($"on event received {body}");
            lock (_dbgEventHandler)
            {
                _dbgEventHandler(this, new DebuggeeEventEventArgs()
                {
                    Event = category,
                    Body = body
                });
            }
        }

        public event EventHandler<DebugeeOutputEventArgs> OutputReceived;
        public event EventHandler ProcessExited;
        public event EventHandler<DebuggeeEventEventArgs> DebugEventReceived
        {
            add
            {
                lock (_dbgEventHandler)
                {
                    _dbgEventHandler += value;
                }
            }
            remove
            {
                lock (_dbgEventHandler)
                {
                    _dbgEventHandler -= value;
                }
            }
        }
        
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
            SendOutput("stdout", e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                _stderrEOF = true;
            }
            SendOutput("stderr", e.Data);
        }

        private void SendOutput(string category, string data)
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
                _listener?.Stop();
            }
        }

        public void Kill()
        {
            _listener?.Stop();

            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }
        }

        public void ListenToEvents()
        {
            _listener = new DebugEventListener(_client, OnDebugEventReceived);
            _listener.Start();
        }
    }
}
