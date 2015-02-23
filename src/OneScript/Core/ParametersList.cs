using System;
using System.Collections.Generic;

namespace OneScript.Core
{
    public class ParametersList : IEnumerable<MethodParameter>
    {
        MethodParameter[] _data;

        public ParametersList()
            : this(new MethodParameter[0])
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