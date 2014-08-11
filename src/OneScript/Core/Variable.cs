using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public class Variable : IVariable
    {
        private IValue _val;

        public Variable()
        {
            this.Value = ValueFactory.Create();
        }

        public Variable(IValue val)
        {
            this.Value = val;
        }

        public virtual IValue Dereference()
        {
            return _val;
        }

        public virtual IValue Value
        {
            get
            {
                return _val;
            }
            set
            {
                var oldCounted = _val as IRefCountable;
                if (oldCounted != null)
                    oldCounted.Release();

                _val = value;

                var newCounted = _val as IRefCountable;
                if (newCounted != null)
                    newCounted.AddRef();

            }
        }

        public DataType Type
        {
            get { return Value.Type; }
        }

        public double AsNumber()
        {
            return Value.AsNumber();
        }

        public string AsString()
        {
            return Value.AsString();
        }

        public DateTime AsDate()
        {
            return Value.AsDate();
        }

        public bool AsBoolean()
        {
            return Value.AsBoolean();
        }

        public IRuntimeContextInstance AsObject()
        {
            return Value.AsObject();
        }

        public bool Equals(IValue other)
        {
            return Value.Equals(other);
        }

        public int CompareTo(IValue other)
        {
            return Value.CompareTo(other);
        }
    }
}
