/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ScriptEngine.HostedScript.Library.Hash
{
    public class CombinedStream : Stream
    {
        private List<Stream> _streams = new List<Stream>();
        private long _position;
        private long[] _borders;

        public CombinedStream()
        { }

        public CombinedStream(Stream[] streams)
        {
            _streams.AddRange(streams);
            CalculateBorders();
        }

        
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return _streams.Sum(x => x.Length);
            }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        public void AddStream(Stream stream)
        {
            _streams.Add(stream);
            CalculateBorders();
        }


        public override void Close()
        {
            foreach (var stream in _streams)
                stream.Close();
        }

        public override void Flush()
        {
            foreach (var stream in _streams)
                stream.Flush();
        }


        public override void SetLength(long position)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.End && offset > 0)
                throw new ArgumentException();
            if (origin == SeekOrigin.Begin && offset < 0)
                throw new ArgumentException();
            if (origin == SeekOrigin.Current && (offset + _position < 0 || offset + _position > Length - 1))
                throw new ArgumentException();
            switch(origin)
            {
                case SeekOrigin.Current:
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    _position = Length - 1 + offset;
                    break;
                case SeekOrigin.Begin:
                    _position = offset;
                    break;
            }
            return _position;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //Где он с винтом?
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            Stream stream = GetCurrentStream();
            if (stream == null)
                return 0;
            int readed = stream.Read(buffer, offset, count);
            _position += readed;
            return readed;
        }

        private void CalculateBorders()
        {
            List<long> tempBorders = new List<long>();
            tempBorders.Add(0);
            long currentBorder = 0;
            foreach(var stream in _streams)
            {
                currentBorder += stream.Length;
                tempBorders.Add(currentBorder);
            }
            _borders = tempBorders.ToArray();
        }

        private Stream GetCurrentStream()
        {
            int index = _borders.Select((val, i) => new { val, i }).Where(x => x.val <= _position).Last().i;
            if (index > _streams.Count - 1)
                return null;
            long border = _borders[index];
            Stream s=_streams[index];
            s.Seek(_position - border, SeekOrigin.Begin);
            return s;
        }
    }
}
