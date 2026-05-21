/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Binary;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OneScript.StandardLibrary.Hash
{
    [ContextClass("ХешированиеДанных", "DataHashing")]
    public class HashImpl : AutoContext<HashImpl>, IDisposable
    {
        private const int BUFFER_SIZE = (1024 * 32);

        private readonly Crc32 _crc32;
        private readonly IncrementalHash _provider;
        private readonly HashFunctionEnum _enumValue;
        private byte[] _hash;

        public HashImpl(IncrementalHash provider, HashFunctionEnum enumValue)
        {
            _provider = provider;
            _enumValue = enumValue;
            if (enumValue == HashFunctionEnum.CRC32)
            {
                _crc32 = new Crc32();
            }
            else ArgumentNullException.ThrowIfNull(provider);

            AppendData(Array.Empty<byte>());
        }

        [ContextProperty("ХешФункция", "HashFunction")]
        public HashFunctionEnum Extension => _enumValue;

        [ContextProperty("ХешСумма", "HashSum")]
        public IValue Hash
        {
            get
            {
                if (_crc32 != null)
                   return ValueFactory.Create(_crc32.GetCurrentHashAsUInt32());
                
                return new BinaryDataContext(_hash);
            }
        }

        [ContextProperty("ХешСуммаСтрокой", "HashSumOfString")]
        public string HashString
        {
            get
            {
                if (_crc32 != null)
                    return _crc32.GetCurrentHashAsUInt32().ToString("X8");

                var sb = new StringBuilder();
                for (int i = 0; i < _hash.Length; i++)
                    sb.Append(_hash[i].ToString("X2"));
                return sb.ToString();
            }
        }

        private void AppendData(byte[] data)
        {
            AppendData(data, data.Length);
        }

        private void AppendData(byte[] data, int count)
        {
            if (_crc32 != null)
            {
                _crc32.AppendData(data,0,count);
            }
            else
            {
                _provider.AppendData(data,0,count);
                _hash = _provider.GetCurrentHash();
            }
        }

        private void AppendStream(Stream stream)
        {
            var buffer = new byte[BUFFER_SIZE];
            while (true)
            {
                var read = stream.Read(buffer,0, BUFFER_SIZE);
                if (read == 0)
                    break;

                AppendData(buffer, read);
            }
         }

        private void AppendStream(Stream stream, int count)
        {
            int bufSize = Math.Min(BUFFER_SIZE, count);
            var buffer = new byte[bufSize];
            int toRead = count;
            while (toRead > 0)
            {
                var read = stream.Read(buffer,0, Math.Min(toRead, bufSize));
                if (read == 0)
                    break;

                AppendData(buffer, read);
                toRead -= read;
            }
        }

        [ContextMethod("Добавить", "Append")]
        public void Append(BslValue toAdd, int count = 0)
        {
            switch (toAdd)
            {
                case BslStringValue s:
                    AppendData(Encoding.UTF8.GetBytes((string)s));
                    break;
                case IStreamWrapper wrapper:
                    var stream = wrapper.GetUnderlyingStream();
                    if (count <= 0)
                        AppendStream(stream);
                    else
                        AppendStream(stream, count);
                    break;
                case BinaryDataContext binaryData:
                    AppendStream(binaryData.GetStream());
                    break;
                default:
                    throw RuntimeException.InvalidNthArgumentType(1);
            }
        }

        [ContextMethod("ДобавитьФайл", "AppendFile")]
        public void AppendFile(string path)
        {
            if (!File.Exists(path))
                throw RuntimeException.InvalidArgumentType();

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            AppendStream(stream);
        }

        [ScriptConstructor(Name = "По указанной хеш-функции")]
        public static HashImpl Constructor(HashFunctionEnum providerEnum)
        {
            if (providerEnum == HashFunctionEnum.CRC32)
                return new HashImpl(null, providerEnum);

            var objectProvider = IncrementalHash.CreateHash(GetAlgorithmName(providerEnum));
            return new HashImpl(objectProvider, providerEnum);
        }

        private static HashAlgorithmName GetAlgorithmName(HashFunctionEnum algo)
        {
            return algo switch
            {
                HashFunctionEnum.MD5 => HashAlgorithmName.MD5,
                HashFunctionEnum.SHA1 => HashAlgorithmName.SHA1,
                HashFunctionEnum.SHA256 => HashAlgorithmName.SHA256,
                HashFunctionEnum.SHA384 => HashAlgorithmName.SHA384,
                HashFunctionEnum.SHA512 => HashAlgorithmName.SHA512,
                _ => throw RuntimeException.InvalidArgumentValue()
            };
        }

        public void Dispose()
        {
            _provider?.Dispose();
        }
    }
}
