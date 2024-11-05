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
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Binary
{
    [ContextClass("ДвоичныеДанные", "BinaryData")]
    public sealed class BinaryDataContext : AutoContext<BinaryDataContext>, IDisposable
    {
        private byte[] _buffer;
        private BackingTemporaryFile _backingFile;

        public BinaryDataContext(string filename)
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            ReadFromStream(fs);
        }

        public BinaryDataContext(byte[] buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public BinaryDataContext(Stream stream)
        {
            long pos = 0;
            ReadFromStream(stream);
            stream.Position = pos;
        }

        /// <summary>
        /// Признак хранения данных в памяти
        /// </summary>
        public bool InMemory => _backingFile == null;
        
        private void ReadFromStream(Stream stream)
        {
            if (stream.Length < FileBackingConstants.DEFAULT_MEMORY_LIMIT)
            {
                LoadToBuffer(stream);
            }
            else
            {
                _buffer = null;
                _backingFile = new BackingTemporaryFile(stream);
            }
        }

        public void Dispose()
        {
            _backingFile?.Dispose();
        }

        private void LoadToBuffer(Stream fs)
        {
            _buffer = new byte[fs.Length];
            // ReSharper disable once MustUseReturnValue
            fs.Read(_buffer, 0, _buffer.Length);
        }

        /// <summary>
        /// Размер двоичных данных в байтах.
        /// </summary>
        [ContextMethod("Размер", "Size")]
        public long Size()
        {
            return _backingFile?.Size() ?? _buffer.Length;
        }

        /// <summary>
        /// Сохранить содержимое двоичных данных в файл или другой поток
        /// </summary>
        /// <param name="filenameOrStream">путь к файлу или Поток</param>
        [ContextMethod("Записать", "Write")]
        public void Write(IValue filenameOrStream)
        {
            if(filenameOrStream.SystemType == BasicTypes.String)
            {
                var filename = filenameOrStream.AsString();

                using(var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    CopyTo(fs);
                }
            }
            else if (filenameOrStream.AsObject() is IStreamWrapper stream)
            {
                 CopyTo(stream.GetUnderlyingStream());
            }
            else
            {
                throw RuntimeException.InvalidArgumentType("filenameOrStream");
            }
        }

        public void CopyTo(Stream stream)
        {
            if (InMemory)
            {
                stream.Write(_buffer, 0, _buffer.Length);
            }
            else
            {
                using var source = _backingFile.OpenReadStream();
                source.CopyTo(stream);
            }
        }

        public Stream GetStream()
        {
            return InMemory ? new MemoryStream(_buffer, 0, _buffer.Length, false, true) : _backingFile.OpenReadStream();
        }

        public byte[] Buffer
        {
            get
            {
                if (!InMemory)
                {
                    using var readStream = _backingFile.OpenReadStream();
                    LoadToBuffer(readStream);
                    _backingFile.Dispose();
                    _backingFile = null;
                }

                return _buffer;
            }
        }

        protected override string ConvertToString()
        {
            const int LIMIT = 64;
            int length;
            byte[] buffer;

            if (InMemory)
            {
                length = Math.Min(_buffer.Length, LIMIT);
                if (length == 0)
                    return "";

                buffer = _buffer;
            }
            else
            {
                length = (int)Math.Min(_backingFile.Size(), LIMIT);
                if (length == 0)
                    return "";

                buffer = new byte[length];
                using var readStream = _backingFile.OpenReadStream();
                // ReSharper disable once MustUseReturnValue
                readStream.Read(buffer, 0, length);
            }
            
            StringBuilder hex = new StringBuilder(length*3);
            hex.AppendFormat("{0:X2}", buffer[0]);
            for (int i = 1; i < length; ++i)
            {
                hex.AppendFormat(" {0:X2}", buffer[i]);
            }

            if (Size() > LIMIT)
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
            return new GenericStream(GetStream(), true);
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

            if (other.SystemType == SystemType)
            {
                var binData = other.GetRawValue() as BinaryDataContext;
                Debug.Assert(binData != null);

                if (InMemory && binData.InMemory)
                {
                    return ArraysAreEqual(_buffer, binData._buffer);
                }
                else
                {
                    using var s1 = GetStream();
                    using var s2 = binData.GetStream();
                    
                    return StreamsAreEqual(s1, s2);
                }
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

        private static bool StreamsAreEqual(Stream s1, Stream s2)
        {
            if (s1.Length == s2.Length)
            {
                for (long i = 0; i < s1.Length; i++)
                {
                    if (s1.ReadByte() != s2.ReadByte())
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
