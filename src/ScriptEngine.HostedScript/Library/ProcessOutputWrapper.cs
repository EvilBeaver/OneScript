using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using sys = System.Diagnostics;

namespace ScriptEngine.HostedScript.Library
{
    class ProcessOutputWrapper : TextReader
    {
        private sys.Process _process;
        private OutputVariant _variant;
        private StringBuilder _buffer = new StringBuilder(4096);
        private ReaderWriterLockSlim _locker;
        
        private int _bufferIndex = 0;

        private bool AlreadyReading { get; set; }

        private Encoding Encoding { get; set; }

        public enum OutputVariant
        {
            Stdout,
            Stderr
        }

        public ProcessOutputWrapper(sys.Process process, OutputVariant variant)
        {
            _process = process;
            _variant = variant;
            _locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        public void StartReading()
        {
            if (AlreadyReading)
                return;

            if (_variant == OutputVariant.Stdout)
            {
                Encoding = _process.StartInfo.StandardOutputEncoding;
                _process.BeginOutputReadLine();
                _process.OutputDataReceived += StreamDataReceived;
            }
            else
            {
                Encoding = _process.StartInfo.StandardErrorEncoding;
                _process.BeginErrorReadLine();
                _process.ErrorDataReceived += StreamDataReceived;
            }

            AlreadyReading = true;
        }

        private void StopReading()
        {
            if (_variant == OutputVariant.Stdout)
            {
                _process.OutputDataReceived -= StreamDataReceived;
            }
            else
            {
                _process.ErrorDataReceived -= StreamDataReceived;
            }
        }

        private void StreamDataReceived(object sender, sys.DataReceivedEventArgs e)
        {
            try
            {
                if (e.Data != null)
                {
                    _locker.EnterWriteLock();
                    if (_buffer.Length != 0)
                        _buffer.Append(System.Environment.NewLine);

                    _buffer.Append(e.Data);
                }
            }
            finally
            {
                if(_locker.IsWriteLockHeld) // При else бросит правильное исключение, из-за которого не захватил блокировку
                    _locker.ExitWriteLock();
            }
        }

        public override int Peek()
        {
            try
            {
                EnterReadLock();
                if (_bufferIndex >= _buffer.Length)
                    return -1; // no data

                return _buffer[_bufferIndex];
            }
            finally
            {
                if (_locker.IsReadLockHeld) // При else бросит правильное исключение, из-за которого не захватил блокировку
                    _locker.ExitReadLock();
            }

        }

        public override int Read()
        {
            try
            {
                EnterReadLock();
                return ReadInternal();
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }
        }

        private int ReadInternal()
        {
            if (_bufferIndex < _buffer.Length)
                return _buffer[_bufferIndex++];

            return -1;
        }

        public override int Read(char[] destBuffer, int index, int count)
        {
            if (destBuffer == null)
                throw new ArgumentNullException(nameof(destBuffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is below zero");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count is below zero");
            if (destBuffer.Length - index < count)
                throw new ArgumentException("Invalid offset");

            try
            {
                EnterReadLock();
                int n = 0;
                do
                {
                    int ch = ReadInternal();
                    if (ch == -1) break;

                    destBuffer[index + n++] = (char) ch;
                } while (n < count);

                return n;
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }
        }

        public override string ReadLine()
        {
            try
            {
                EnterReadLock();
                var sb = new StringBuilder();
                while (true)
                {
                    int ch = ReadInternal();
                    if (ch == -1) break;
                    if (ch == '\r' || ch == '\n')
                    {
                        if (ch == '\r' && Peek() == '\n') Read();
                        return sb.ToString();
                    }
                    sb.Append((char)ch);
                }
                if (sb.Length > 0) return sb.ToString();
                return null; 
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }
        }

        public override string ReadToEnd()
        {
            try
            {
                EnterReadLock();
                string data = base.ReadToEnd();
                ResetBuffer();
                return data;
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }
        }

        private void ResetBuffer()
        {
            _buffer.Clear();
            _bufferIndex = 0;
        }

        private void EnterReadLock()
        {
            if (_process.HasExited)
            {
                _process.WaitForExit(); // ожидание закрытия потоков
            }

            _locker.EnterReadLock();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopReading();
            }

            base.Dispose(disposing);
        }
    }
}
