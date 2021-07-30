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
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Binary
{
    [ContextClass("ДвоичныеДанные", "BinaryData")]
    public class BinaryDataContext : AutoContext<BinaryDataContext>, IDisposable
    {
        private const int INMEMORY_LIMIT = 1024 * 1024 * 50; // 50 Mb

        private byte[] _buffer;
        
        FileStream _backingFile;

        public bool InMemory => _backingFile == null;

        public BinaryDataContext(string filename)
        {
            using(var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                ReadFromStream(fs);
            }
        }

        public BinaryDataContext(byte[] buffer)
        {
            _buffer = buffer;
        }

        public BinaryDataContext(Stream stream)
        {
            var pos = stream.Position;
            ReadFromStream(stream);
            stream.Position = pos;
        }

        private void ReadFromStream(Stream stream)
        {
            if (stream.Length < INMEMORY_LIMIT)
            {
                LoadToBuffer(stream);
            }
            else
            {
                _buffer = null;
                var backingFileName = Path.GetTempFileName();
                _backingFile = new FileStream(backingFileName, FileMode.Create);
                stream.CopyTo(_backingFile);
            }
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
                _buffer = null;
            }
            DeleteTemporaryFile();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~BinaryDataContext()
        {
            Dispose(false);
        }

        private void LoadToBuffer(Stream fs)
        {
            _buffer = new byte[fs.Length];
            fs.Read(_buffer, 0, _buffer.Length);
        }

        private void DeleteTemporaryFile()
        {
            if (_backingFile != null && File.Exists(_backingFile.Name))
            {
                try
                {
                    _backingFile.Close();
                    File.Delete(_backingFile.Name);
                }
                catch
                {
                    SystemLogger.Write($"WARNING! Can't delete temporary file {_backingFile.Name}");
                }
                finally
                {
                    _backingFile = null;
                }
            }
        }

        [ContextMethod("Размер", "Size")]
        public long Size()
        {
            return _backingFile?.Length ?? _buffer?.Length ??  0;
        }

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
                _backingFile.Seek(0, SeekOrigin.Begin);
                _backingFile.CopyTo(stream);
            }
        }

        public Stream GetStream()
        {
            if (InMemory)
            {
                return new MemoryStream(_buffer, 0, _buffer.Length, false, true);
            }
            else
            {
                _backingFile.Seek(0,SeekOrigin.Begin);
                return _backingFile;
            }
        }

        public byte[] Buffer
        {
            get
            {
                if (!InMemory)
                {
                    _backingFile.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        LoadToBuffer(_backingFile);
                    }
                    finally
                    {
                        DeleteTemporaryFile();
                    }
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
                length = (int)Math.Min(_backingFile.Length, LIMIT);
                if (length == 0)
                    return "";

                buffer = new byte[length];
                _backingFile.Seek(0, SeekOrigin.Begin);
                _backingFile.Read(buffer, 0, length);
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
                   return ArraysAreEqual(_buffer, binData._buffer);
                else
                   return StreamsAreEqual(GetStream(), binData.GetStream());
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
