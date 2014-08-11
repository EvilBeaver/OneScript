using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public class Reference : IVariable
    {
        IVariable _referencedVariable;

        public Reference(IVariable referenced)
        {
            _referencedVariable = referenced;
        }

        public IValue Value
        {
            get
            {
                return _referencedVariable.Value;
            }
            set
            {
                _referencedVariable.Value = value;
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
            get { return _referencedVariable.Type; }
        }

        public double AsNumber()
        {
            return _referencedVariable.AsNumber();
        }

        public string AsString()
        {
            return _referencedVariable.AsString();
        }

        public DateTime AsDate()
        {
            return _referencedVariable.AsDate();
        }

        public bool AsBoolean()
        {
            return _referencedVariable.AsBoolean();
        }

        public IRuntimeContextInstance AsObject()
        {
            return _referencedVariable.AsObject();
        }

        public bool Equals(IValue other)
        {
            return _referencedVariable.Equals(other);
        }

        public int CompareTo(IValue other)
        {
            return _referencedVariable.CompareTo(other);
        }
    }
}
