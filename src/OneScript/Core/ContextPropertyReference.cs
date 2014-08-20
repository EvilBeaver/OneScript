using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    class ContextPropertyReference : IVariable
    {
        private IRuntimeContextInstance _context;
        private int _index;

        public ContextPropertyReference(IRuntimeContextInstance ctx, int idx)
        {
            _context = ctx;
            _index = idx;
        }

        public IValue Value
        {
            get
            {
                return _context.GetPropertyValue(_index);
            }
            set
            {
                _context.SetPropertyValue(_index, value);
            }
        }

        public IValue Dereference()
        {
            IValue realValue = Value;
            var reference = realValue as IVariable;
            if (reference != null)
                return reference.Dereference();
            else
                return realValue;
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
