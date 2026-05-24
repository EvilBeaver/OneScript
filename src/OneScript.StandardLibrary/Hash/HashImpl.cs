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
    /// <summary>
    /// Реализует инкрементальный расчет хеш-суммы по добавленным данным.
    /// Тип вычисляемого значения определяются типом хеш-функции.
    /// </summary>
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

        /// <summary>
        /// Вид хеш-функции, определяющий способ вычисления хеш-суммы.
        /// Только для чтения
        /// </summary>
        /// <value>Перечисление ХешФункция</value>
        [ContextProperty("ХешФункция", "HashFunction")]
        public HashFunctionEnum HashFunction => _enumValue;

        /// <summary>
        /// Текущее значение хеш-суммы. Только для чтения
        /// </summary>
        /// <value>Для хеш-функции CRC32 - Число, для остальных - ДвоичныеДанные
        /// </value>
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

        /// <summary>
        /// Нестандартное расширение!
        /// Строковое представление текущего значения хеш-суммы. Только для чтения
        /// </summary>
        /// <value>Для хеш-функции CRC32 - Число, для остальных - ДвоичныеДанные</value>
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
            if (count <= 0)
            {
                AppendStream(stream);
                return;
            }

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

        /// <summary>
        /// Добавляет данные и  обновляет хеш-сумму
        /// </summary>
        /// <param name="toAdd">Источник данных. Строка, ДвоичныеДанные или Поток</param>
        /// <param name="count">Для источника данных типов Строка или ДвоичныеДанные - игнорируется.
        /// Для источника данных типа Поток - Количество байтов, которые читаются из потока.
        /// Если количество не задано, нулевое <b>или отрицательное</b>, то читаются все данные до конца потока.
        /// </param>
        [ContextMethod("Добавить", "Append")]
        public void Append(BslValue toAdd, BslValue count = null)
        {
            switch (toAdd)
            {
                case BslStringValue s:
                    AppendData(Encoding.UTF8.GetBytes((string)s));
                    break;
                case BinaryDataContext binaryData:
                    AppendStream(binaryData.GetStream());
                    break;

                case IStreamWrapper wrapper:
                    var stream = wrapper.GetUnderlyingStream();
                    if (count == null)
                    {
                        AppendStream(stream);
                    }
                    else
                    {
                        int cnt;
                        try
                        {
                            cnt = (int)count;
                        }
                        catch
                        {
                            if (count is BslStringValue)
                                throw RuntimeException.InvalidNthArgumentValue(2);
                            else
                                throw RuntimeException.InvalidNthArgumentType(2);
                        }

                        AppendStream(stream, cnt);
                    }
                    break;

                default:
                    throw RuntimeException.InvalidNthArgumentType(1);
            }
        }

        /// <summary>
        /// Добавляет двоичные данные из файла и обновляет хеш-сумму
        /// </summary>
        /// <param name="path">Имя файла, из которого читаются данные. Тип: Строка</param>
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
