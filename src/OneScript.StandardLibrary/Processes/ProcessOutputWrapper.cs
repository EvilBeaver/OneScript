/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;
using System.Threading;
using sys = System.Diagnostics;

namespace OneScript.StandardLibrary
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
                _process.OutputDataReceived += StreamDataReceived;
                _process.BeginOutputReadLine();
            }
            else
            {
                Encoding = _process.StartInfo.StandardErrorEncoding;
                _process.ErrorDataReceived += StreamDataReceived;
                _process.BeginErrorReadLine();
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
            if (e.Data != null)
            {
                lock(_buffer)
                {
                    if (_buffer.Length != 0)
                        _buffer.Append(System.Environment.NewLine);

                    _buffer.Append(e.Data);
                }
            }
        }

        public override int Peek()
        {
            lock (_buffer)
            {
                if (_bufferIndex >= _buffer.Length)
                    return -1; // no data

                return _buffer[_bufferIndex]; 
            }
        }

        public override int Read()
        {
            lock (_buffer)
            {
                return ReadInternal();
            }
        }

        // неблокирующий доступ к буферу.
        // должна вызываться ТОЛЬКО внутри вышестоящего блока lock.
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

           
            int n = 0;
            lock (_buffer)
            {
                do
                {
                    int ch = ReadInternal();
                    if (ch == -1) break;

                    destBuffer[index + n++] = (char)ch;
                } while (n < count); 
            }

            return n;
        }

        public override string ReadLine()
        {
            var sb = new StringBuilder();
            lock (_buffer)
            {
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
            }
            if (sb.Length > 0)
                return sb.ToString();

            return null; 
        }

        public override string ReadToEnd()
        {
            lock (_buffer)
            {
                string data = base.ReadToEnd();
                ResetBuffer();
                return data; 
            }
        }

        private void ResetBuffer()
        {
            _buffer.Clear();
            _bufferIndex = 0;
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
