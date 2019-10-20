/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Hash
{
    [ContextClass("ХешированиеДанных", "DataHashing")]
    public class HashImpl : AutoContext<HashImpl>
    {
        private readonly HashAlgorithm _provider;
        private readonly IValue _hashFunction;
        private byte[] _hash;

        public HashImpl(HashAlgorithm provider, IValue hashFunction)
        {
            _provider = provider;
            _hashFunction = hashFunction;
        }

        [ContextProperty("ХешФункция", "HashFunction")]
        public IValue HashFunction => _hashFunction;

        [ContextProperty("ХешСумма", "HashSum")]
        public IValue Hash
        {
            get
            {
                if (_provider is Crc32)
                {
                    var buffer = new byte[4];
                    Array.Copy(_hash, buffer, 4);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(buffer);
                    var ret = BitConverter.ToUInt32(buffer, 0);
                    return ValueFactory.Create(ret);
                }

                return new BinaryDataContext(_hash);
            }
        }

        [ContextProperty("ХешСуммаСтрокой", "HashSumOfString")]
        public string HashString => BitConverter.ToString(_hash).Replace("-", "");


        [ContextMethod("Добавить", "Append")]
        public void Append(IValue data, uint count = 0)
        {
            switch (data.DataType)
            {
                case DataType.String:
                    _hash = _provider.ComputeHash(Encoding.UTF8.GetBytes(data.AsString()));
                    break;
                case DataType.Object:
                    switch (data)
                    {
                        case GenericStream stream:
                        {
                            var underlyingStream = stream.GetUnderlyingStream();
                            if (count == 0) // Читать все до конца потока
                                _hash = _provider.ComputeHash(underlyingStream);
                            else
                            {
                                const int bufferSize = 4096;
                                var buffer = new byte[bufferSize];
                                var remain = Convert.ToInt32(count);
                                var readBytes = 0;
                                do
                                {
                                    readBytes = underlyingStream.Read(buffer, 0, Math.Min(bufferSize, remain));
                                    if (readBytes > 0)
                                        _hash = _provider.ComputeHash(buffer, 0, readBytes);
                                    remain -= readBytes;
                                } while (readBytes > 0);
                            }

                            break;
                        }
                        case BinaryDataContext binaryData:
                            _hash = _provider.ComputeHash(binaryData.Buffer);
                            break;
                    }

                    break;
                default:
                    throw RuntimeException.InvalidArgumentType();
            }
        }

        [ContextMethod("ДобавитьФайл", "AppendFile")]
        public void AppendFile(string path)
        {
            if (!File.Exists(path))
                throw RuntimeException.InvalidArgumentType();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                _hash = _provider.ComputeHash(stream);
            }
        }

        [ScriptConstructor(Name = "По указанной хеш-функции")]
        public static HashImpl Constructor(IValue hashFunction)
        {
            var objectProvider = HashFunctionEnum.GetProvider(hashFunction);
            return new HashImpl(objectProvider, hashFunction);
        }
    }
}
