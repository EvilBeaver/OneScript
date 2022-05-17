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
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.StandardLibrary.Binary;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Hash
{
    [ContextClass("ХешированиеДанных", "DataHashing")]
    public class HashImpl : AutoContext<HashImpl>, IDisposable
    {
        private HashAlgorithm _provider;
        private HashFunctionEnum _enumValue;
        private CombinedStream _toCalculate=new CombinedStream();
        private bool _calculated;
        private byte[] _hash;

        public HashImpl(HashAlgorithm provider, HashFunctionEnum enumValue)
        {
            _provider = provider;
            _enumValue = enumValue;
            _calculated = false;
        }

        public byte[] InternalHash
        {
            get
            {
                if (!_calculated)
                {
                    _hash = _provider.ComputeHash(_toCalculate);
                    _calculated = true;
                }
                return _hash;
            }
        }

        [ContextProperty("ХешФункция", "HashFunction")]
        public HashFunctionEnum Extension => _enumValue;

        [ContextProperty("ХешСумма", "HashSum")]
        public IValue Hash
        {
            get
            {
                if (_provider is Crc32)
                {
                    var buffer = new byte[4];
                    Array.Copy(InternalHash, buffer, 4);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(buffer);
                    var ret = BitConverter.ToUInt32(buffer, 0);
                    return ValueFactory.Create((decimal)ret);
                }
                return new BinaryDataContext(InternalHash);
            }
        }

        [ContextProperty("ХешСуммаСтрокой", "HashSumOfString")]
        public string HashString
        {
            get
            {
                var sb = new StringBuilder();
                for (int i = 0; i < InternalHash.Length; i++)
                    sb.Append(InternalHash[i].ToString("X2"));
                return sb.ToString();
            }
        }


        [ContextMethod("Добавить", "Append")]
        public void Append(IValue toAdd, uint count = 0)
        {
            var realValue = toAdd.GetRawValue();
            switch (realValue)
            {
                case BslStringValue s:
                    AddStream(new MemoryStream(Encoding.UTF8.GetBytes((string)s)));
                    break;
                case BslObjectValue obj when obj is IStreamWrapper wrapper:
                    var stream = wrapper.GetUnderlyingStream();
                    var readByte = (int)Math.Min(count == 0 ? stream.Length : count, stream.Length - stream.Position);
                    var buffer = new byte[readByte];
                    stream.Read(buffer, 0, readByte);
                    AddStream(new MemoryStream(buffer));
                    break;
                case BslObjectValue obj when obj is BinaryDataContext binaryData:
                    AddStream(binaryData.GetStream());
                    break;
                default:
                    throw RuntimeException.InvalidArgumentType(nameof(toAdd));
            }
        }

        [ContextMethod("ДобавитьФайл", "AppendFile")]
        public void AppendFile(string path)
        {
            if (!File.Exists(path))
                throw RuntimeException.InvalidArgumentType();
            AddStream(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _toCalculate.Close();
            _toCalculate.Dispose();
            _toCalculate = new CombinedStream();
            _calculated = false;
        }


        [ScriptConstructor(Name = "По указанной хеш-функции")]
        public static HashImpl Constructor(HashFunctionEnum providerEnum)
        {
            var objectProvider = GetProvider(providerEnum);
            return new HashImpl(objectProvider, providerEnum);
        }

        private static HashAlgorithm GetProvider(HashFunctionEnum algo)
        {
            switch (algo)
            {
                case HashFunctionEnum.CRC32:
                    return new Crc32();
                default:
                    var ret = HashAlgorithm.Create(algo.ToString());
                    if (ret == null)
                        throw RuntimeException.InvalidArgumentType();
                    return ret;
            }
        }

        public void Dispose()
        {
            _toCalculate.Close();
            _toCalculate.Dispose();
        }

        private void AddStream(Stream stream)
        {
            _toCalculate.AddStream(stream);
            _toCalculate.Seek(0, SeekOrigin.Begin);
            _calculated = false;
            
        }
    }
}
