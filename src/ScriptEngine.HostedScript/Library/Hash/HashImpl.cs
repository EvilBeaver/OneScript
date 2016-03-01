using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Security.Cryptography;
using System.IO;
using ScriptEngine.HostedScript.Library;

namespace ScriptEngine.HostedScript.Library.Hash
{
    [ContextClass("ХешированиеДанных", "DataHashing")]
    class HashImpl: AutoContext<HashImpl>
    {
        protected HashAlgorithm _provider;
        protected IValue _enumValue;
        protected CombinedStream _toCalculate=new CombinedStream();
        protected bool _calculated;
        protected string _tempFileName;
        protected byte[] _hash;

        public HashImpl(HashAlgorithm provider, IValue enumValue)
        {
            _provider = provider;
            _enumValue = enumValue;
            _calculated = false;
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
        public IValue Hash
        {
            get
            {
                if (!_calculated)
                {
                    _hash = _provider.ComputeHash(_toCalculate);
                    _calculated = true;
                }
                return new BinaryDataContext(_hash);
            }
        }

        [ContextProperty("ХешСуммаСтрокой", "HashSumOfString")]
        public string HashString
        {
            get
            {
                if (!_calculated)
                {
                    _hash = _provider.ComputeHash(_toCalculate);
                    _calculated = true;
                }
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < _hash.Length; i++)
                    sb.Append(_hash[i].ToString("X2"));
                return sb.ToString();
            }
        }


        [ContextMethod("Добавить", "Append")]
        public void Append(IValue toAdd)
        {
            byte[] buffer = null;
            if (toAdd.DataType==DataType.String)
            {
                buffer = Encoding.UTF8.GetBytes(toAdd.AsString());
            }
            else if(toAdd.DataType==DataType.Object)
            {
                try
                {
                    var binaryData = toAdd as BinaryDataContext;
                    buffer = binaryData.Buffer;
                }
                catch
                {
                     throw RuntimeException.InvalidArgumentType();
                }
            }
            AddStream(new MemoryStream(buffer));
            
        }

        [ContextMethod("ДобавитьФайл", "AppendFile")]
        public void AppendFile(string path)
        {
            if (!File.Exists(path))
                throw RuntimeException.InvalidArgumentType();
            AddStream(new FileStream(path, FileMode.Open));
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _toCalculate.Close();
            _toCalculate.Dispose();
            _toCalculate = new CombinedStream();
            _calculated = false;
        }


        [ScriptConstructor(Name = "По хэш-функции")]
        public static IRuntimeContextInstance Constructor(IValue providerEnum)
        {
            var objectProvider = HashFunctionEnum.GetProvider(providerEnum);
            return new HashImpl(objectProvider, providerEnum);
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
