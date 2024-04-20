/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using ScriptEngine;

namespace OneScript.StandardLibrary.Binary
{
    /// <summary>
    /// Поток, который хранит данные в памяти до определенного лимита, потом во временном файле
    /// </summary>
    public class FileBackingStream : Stream
    {
        private readonly int _inMemoryLimit;
        private Stream _backingStream;

        private string _backingFileName;
        
        public FileBackingStream() : this(FileBackingConstants.DEFAULT_MEMORY_LIMIT)
        {
        }

        public FileBackingStream(int inMemoryLimit, int capacity = 0)
        {
            if (inMemoryLimit == FileBackingConstants.SYSTEM_IN_MEMORY_LIMIT)
                throw new ArgumentException("Use MemoryStream instead");
            
            _inMemoryLimit = inMemoryLimit;
            _backingStream = new MemoryStream(capacity);
        }
        
        public override void Flush()
        {
            _backingStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _backingStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _backingStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (value <= _inMemoryLimit)
            {
                _backingStream.SetLength(value);
                SwitchToMemory();
            }
            else
            {
                SwitchToFile();
                _backingStream.SetLength(value);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Grow(count);
            _backingStream.Write(buffer, offset, count);
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _backingStream.Length;

        public override long Position
        {
            get => _backingStream.Position;
            set => _backingStream.Position = value;
        }

        public bool HasBackingFile => _backingFileName != null;

        public void SwitchToMemory()
        {
            if (!HasBackingFile)
                return;

            if (_backingStream.Length > _inMemoryLimit)
            {
                throw new InvalidOperationException(
                    $"Size {_backingStream.Length} is larger than limit {_inMemoryLimit}");
            }

            var memStream = new MemoryStream((int)_backingStream.Length);
            var currentPosition = _backingStream.Position;
            _backingStream.Position = 0;
            _backingStream.CopyTo(memStream);
            memStream.Position = currentPosition;
            _backingStream.Close();
            
            DeleteBackingFile();
            _backingStream = memStream;
        }
        
        public void SwitchToFile()
        {
            if (HasBackingFile)
                return;
            
            var currentPosition = _backingStream.Position;
            _backingFileName = Path.GetTempFileName();
            var fileStream = new FileStream(_backingFileName, FileMode.Create);
            try
            {
                _backingStream.Position = 0;
                _backingStream.CopyTo(fileStream);
                fileStream.Position = currentPosition;
                _backingStream.Dispose();
                _backingStream = fileStream;
            }
            catch
            {
                fileStream.Dispose();
                DeleteBackingFile();
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _backingStream.Dispose();
            DeleteBackingFile();
        }
        
        private void Grow(int amount)
        {
            if (HasBackingFile)
                return;
            
            var newSize = _backingStream.Position + amount;
            if (newSize > _inMemoryLimit)
            {
                SwitchToFile();
            }
        }

        private void DeleteBackingFile()
        {
            if (!HasBackingFile) return;
            
            if (File.Exists(_backingFileName))
            {
                try
                {
                    File.Delete(_backingFileName);
                }
                catch
                {
                    SystemLogger.Write($"WARNING! Can't delete temporary file {_backingFileName}");
                }
            }
            _backingFileName = null;
        }
        
        ~FileBackingStream()
        {
            Dispose(false);
        }
    }
}