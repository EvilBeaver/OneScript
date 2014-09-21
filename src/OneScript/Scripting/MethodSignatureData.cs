using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class MethodSignatureData
    {
        private ParametersList _paramList;
        private bool _isFunction;

        private MethodSignatureData()
        {

        }

        public bool IsFunction
        {
            get
            {
                return _isFunction;
            }
        }

        public ParametersList Parameters
        {
            get
            {
                return _paramList;
            }
        }

        public static MethodSignatureData CreateProcedure(ParametersList parameters)
        {
            return new MethodSignatureData()
            {
                _paramList = parameters,
                _isFunction = false
            };
        }

        public static MethodSignatureData CreateFunction(ParametersList parameters)
        {
            return new MethodSignatureData()
            {
                _paramList = parameters,
                _isFunction = true
            };
        }

        public static MethodSignatureData CreateProcedure(int paramCount)
        {
            return CreateProcedure(ParametersList.CreateDefault(paramCount));
        }

        public static MethodSignatureData CreateFunction(int paramCount)
        {
            return CreateFunction(ParametersList.CreateDefault(paramCount));
        }

    }

    /// <summary>
    /// Универсальный описатель параметра. Используется для проверки корректности вызова метода.
    /// Описатель не содержит конкретного значения-по-умолчанию, поскольку используется 
    /// для описаний методов вообще, а не только тех, что объявлены в скрипте.
    /// </summary>
    public struct MethodParameter
    {
        public bool IsOptional;
        public bool IsByValue;
    }

    public class ParametersList : IEnumerable<MethodParameter>
    {
        MethodParameter[] _data;

        public ParametersList() : this(new MethodParameter[0])
        {
        }

        public ParametersList(MethodParameter[] data)
        {
            if (data == null)
                throw new ArgumentNullException();

            _data = data;
        }

        public MethodParameter this[int index]
        {
            get
            {
                return _data[index];
            }
        }

        public int Count
        {
            get
            {
                return _data.Length;
            }
        }

        public IEnumerator<MethodParameter> GetEnumerator()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                yield return _data[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public static ParametersList CreateDefault(int count)
        {
            var arr = new MethodParameter[count];
            return new ParametersList(arr);
        }
    }
}
