/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.IO;
using System.Threading;

namespace OneScript.StandardLibrary.Binary
{
    class StreamWithTimeout : Stream
    {
        private readonly Stream _underlyingStream;
        private int _readTimeout;

        public override bool CanRead => _underlyingStream.CanRead;

        public override bool CanSeek => _underlyingStream.CanSeek;

        public override bool CanWrite => _underlyingStream.CanWrite;

        public override bool CanTimeout => true;

        public override long Length => _underlyingStream.Length;

        public override long Position
        {
            get
            {
                return _underlyingStream.Position;
            }
            set
            {
                _underlyingStream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return _readTimeout;
            }
            set
            {
                _readTimeout = value;
                if (_underlyingStream.CanTimeout)
                    _underlyingStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get => _underlyingStream.WriteTimeout;
            set => _underlyingStream.WriteTimeout = value;
        }

        public override void Flush()
        {
            _underlyingStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_readTimeout > 0 && !_underlyingStream.CanTimeout)
            {
                return ReadWithTimeout(buffer, offset, count);
            }
            else
                return _underlyingStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _underlyingStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _underlyingStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _underlyingStream.Write(buffer, offset, count);
        }

        private int ReadWithTimeout(byte[] buffer, int offset, int count)
        {
            int read = 0;

            AutoResetEvent gotInput = new AutoResetEvent(false);
            Thread inputThread = new Thread(() =>
            {
                try
                {
                    read = _underlyingStream.Read(buffer, offset, count);
                    gotInput.Set();
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
            })
            {
                IsBackground = true
            };

            inputThread.Start();

            // Timeout expired?
            if (!gotInput.WaitOne(_readTimeout))
            {
                inputThread.Abort();
            }

            return read;

        }

        public StreamWithTimeout(Stream underlyingStream)
        {
            _underlyingStream = underlyingStream;
        }
    }
}
