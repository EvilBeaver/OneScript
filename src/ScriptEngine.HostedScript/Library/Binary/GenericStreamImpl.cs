using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.Binary
{
    internal class GenericStreamImpl
    {
        private Stream _underlyingStream;

        public GenericStreamImpl(Stream stream)
        {
            _underlyingStream = stream;
        }

        public void Write(BinaryDataBuffer buffer, int positionInBuffer, int number)
        {
            buffer.ThrowIfReadonly();
            _underlyingStream.Write(buffer.Bytes, positionInBuffer, number);
        }

        public void CopyTo(IValue targetStream, int bufferSize = 0)
        {
            IStreamWrapper sw = targetStream.GetRawValue() as IStreamWrapper;
            if (sw == null)
                throw RuntimeException.InvalidArgumentType("targetStream");

            var stream = sw.GetUnderlyingStream();
            if (bufferSize == 0)
                _underlyingStream.CopyTo(stream);
            else
                _underlyingStream.CopyTo(stream, bufferSize);
        }

        public long Seek(int offset, StreamPositionEnum initialPosition = StreamPositionEnum.Begin)
        {
            SeekOrigin origin;
            switch (initialPosition)
            {
                case StreamPositionEnum.End:
                    origin = SeekOrigin.End;
                    break;
                case StreamPositionEnum.Current:
                    origin = SeekOrigin.Current;
                    break;
                default:
                    origin = SeekOrigin.Begin;
                    break;
            }

            return _underlyingStream.Seek(offset, origin);
        }

        public GenericStream GetReadonlyStream()
        {
            return new GenericStream(_underlyingStream, true);
        }

        public long Read(BinaryDataBuffer buffer, int positionInBuffer, int number)
        {
            return _underlyingStream.Read(buffer.Bytes, positionInBuffer, number);
        }

        public long Size()
        {
            return _underlyingStream.Length;
        }

        public void Flush()
        {
            _underlyingStream.Flush();
        }

        public long CurrentPosition()
        {
            return _underlyingStream.Position;
        }

        public void SetSize(long size)
        {
            _underlyingStream.SetLength(size);
        }

    }
}
