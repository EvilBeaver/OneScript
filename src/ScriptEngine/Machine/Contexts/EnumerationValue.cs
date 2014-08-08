using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    abstract public class EnumerationValue : IValue
    {
        EnumerationContext _owner;

        public EnumerationValue(EnumerationContext owner)
        {
            _owner = owner;
        }

        public EnumerationContext Owner
        {
            get
            {
                return _owner;
            }
        }

        public virtual DataType DataType
        {
            get { return Machine.DataType.GenericValue; }
        }

        public virtual TypeDescriptor SystemType
        {
            get { return _owner.ValuesType; }
        }

        public virtual double AsNumber()
        {
            throw RuntimeException.ConvertToNumberException();
        }

        public virtual DateTime AsDate()
        {
            throw RuntimeException.ConvertToDateException();
        }

        public virtual bool AsBoolean()
        {
            throw RuntimeException.ConvertToBooleanException();
        }

        public virtual string AsString()
        {
            return SystemType.Name;
        }

        public virtual TypeDescriptor AsType()
        {
            throw new NotImplementedException();
        }

        public virtual IRuntimeContextInstance AsObject()
        {
            throw RuntimeException.ValueIsNotObjectException();
        }

        public IValue GetRawValue()
        {
            return this;
        }

        public virtual int CompareTo(IValue other)
        {
            if (other != null)
            {
                if (other is EnumerationValue)
                {
                    int thisIdx = _owner.IndexOf(this);
                    int otherIdx = _owner.IndexOf((EnumerationValue)other);
                    return thisIdx - otherIdx;
                }
                else
                {
                    return SystemType.ID - other.SystemType.ID;
                }
            }
            else
            {
                return 1;
            }
        }

        public virtual bool Equals(IValue other)
        {
            return other == this;
        }
    }
}
