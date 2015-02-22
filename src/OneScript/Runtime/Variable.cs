using System;
using OneScript.Core;

namespace OneScript.Runtime
{
    public class Variable : IVariable
    {
        private IValue _val;

        public Variable()
        {
            _val = ValueFactory.Create();
        }

        public IValue Value
        {
            get
            {
                return _val;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _val = value;
            }
        }

        public IValue Dereference()
        {
            return _val;
        }

        public static IVariable Create(IValue value)
        {
            return new Variable()
            {
                Value = value
            };
        }

        public static Variable[] CreateArray(int len)
        {
            Variable[] result = new Variable[len];
            for (int i = 0; i < len; i++)
            {
                result[i] = new Variable();
            }

            return result;
        }

        public bool Equals(IValue other)
        {
            return this.Value.Equals(other);
        }
    }
}
