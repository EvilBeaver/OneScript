/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Binary
{
    [ContextClass("ДвоичныеДанные", "BinaryData")]
    public class BinaryDataContext : AutoContext<BinaryDataContext>, IDisposable
    {
        private byte[] _buffer;

        public BinaryDataContext(string filename)
        {
            using(var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                _buffer = new byte[fs.Length];
                fs.Read(_buffer, 0, _buffer.Length);
            }
        }

        public BinaryDataContext(byte[] buffer)
        {
            _buffer = buffer;
        }

        public void Dispose()
        {
            _buffer = null;
        }

        [ContextMethod("Размер","Size")]
        public int Size()
        {
            return _buffer.Length;
        }

        [ContextMethod("Записать","Write")]
        public void Write(IValue filenameOrStream)
        {
            if(filenameOrStream.DataType == DataType.String)
            {
                var filename = filenameOrStream.AsString();
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(_buffer, 0, _buffer.Length);
                }
            }
            else if(filenameOrStream.AsObject() is IStreamWrapper stream)
            {
                stream.GetUnderlyingStream().Write(_buffer, 0, _buffer.Length);
            }
            else
            {
                throw RuntimeException.InvalidArgumentType("filenameOrStream");
            }
        }


        public byte[] Buffer => _buffer;

        public override string AsString()
        {
            if (_buffer.Length == 0)
                return "";

            const int LIMIT = 64;
            int length = Math.Min(_buffer.Length, LIMIT);
            
            StringBuilder hex = new StringBuilder(length*3);
            hex.AppendFormat("{0:X2}", _buffer[0]);
            for (int i = 1; i < length; ++i)
            {
                hex.AppendFormat(" {0:X2}", _buffer[i]);
            }

            if (_buffer.Length > LIMIT)
                hex.Append('…');

            return hex.ToString();
        }

        /// <summary>
        /// 
        /// Открывает поток для чтения двоичных данных.
        /// </summary>
        ///
        ///
        /// <returns name="Stream">
        /// Представляет собой поток данных, который можно последовательно читать и/или в который можно последовательно писать. 
        /// Экземпляры объектов данного типа можно получить с помощью различных методов других объектов.</returns>
        ///
        [ContextMethod("ОткрытьПотокДляЧтения", "OpenStreamForRead")]
        public GenericStream OpenStreamForRead()
        {
            var stream = new MemoryStream(_buffer, 0, _buffer.Length, false, true);
            return new GenericStream(stream, true);
        }

        [ScriptConstructor(Name = "На основании файла")]
        public static BinaryDataContext Constructor(IValue filename)
        {
            return new BinaryDataContext(filename.AsString());
        }

        public override bool Equals(IValue other)
        {
            if (other == null)
                return false;

            if (other.SystemType.ID == SystemType.ID)
            {
                var binData = other.GetRawValue() as BinaryDataContext;
                Debug.Assert(binData != null);

                return ArraysAreEqual(_buffer, binData._buffer);
            }

            return false;
        }

        private static bool ArraysAreEqual(byte[] a1, byte[] a2)
        {
            if (a1.LongLength == a2.LongLength)
            {
                for (long i = 0; i < a1.LongLength; i++)
                {
                    if (a1[i] != a2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
