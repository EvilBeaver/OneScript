using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Security.Cryptography;
using System.IO;

namespace ScriptEngine.HostedScript.Library.Hash
{
    [ContextClass("ХешированиеДанных", "DataHashing")]
    class HashImpl: AutoContext<HashImpl>
    {
        protected HashAlgorithm _provider;
        protected IValue _enumValue;
        protected Stream _toCalculate;
        protected bool _inMemory;
        protected string _tempFileName;

        public HashImpl(HashAlgorithm provider, IValue enumValue)
        {
            _provider = provider;
            _enumValue = enumValue;
            _toCalculate = new MemoryStream();
            _inMemory = true;
        }


        [ContextProperty("ХешФункция", "HashFunction")]
        public IValue Extension
        {
            get
            {
                return _enumValue;
            }
        }

        [ContextProperty("ХешСумма", "HashSum")]
        public string Hash
        {
            get
            {
                _toCalculate.Seek(0, SeekOrigin.Begin);
                byte[] hash = _provider.ComputeHash(_toCalculate);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }


        [ContextMethod("Добавить", "Append")]
        public void Append(string text)
        {
            _toCalculate.Seek(0, SeekOrigin.End);
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            _toCalculate.Write(buffer, 0, buffer.Length);
        }

        [ContextMethod("ДобавитьФайл", "AppendFile")]
        public void AppendFile(string path)
        {
            if (_inMemory)
            {
                _tempFileName = Path.GetTempFileName();
                Stream fileStream = new FileStream(_tempFileName, FileMode.Create);
                _toCalculate.Seek(0, SeekOrigin.Begin);
                _toCalculate.CopyTo(fileStream);
                _toCalculate.Close();
                _toCalculate.Dispose();
                _inMemory = false;
                _toCalculate = fileStream;
            }
            using (Stream inputStream = new FileStream(path, FileMode.Open))
            {
                _toCalculate.Seek(0, SeekOrigin.End);
                inputStream.CopyTo(_toCalculate);
            }
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _toCalculate.Close();
            _toCalculate.Dispose();
            if (!_inMemory)
                File.Delete(_tempFileName);
            _toCalculate = new MemoryStream();
            _inMemory = true;
        }


        [ScriptConstructor(Name = "По хэш-функции")]
        public static IRuntimeContextInstance Constructor(IValue providerEnum)
        {
            var objectProvider = HashFunctionEnum.GetProvider(providerEnum);
            return new HashImpl(objectProvider, providerEnum);
        }
    }
}
