using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.Binary
{
    class GenericStreamImpl
    {
        Stream _underlyingStream;

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

    }
}
