using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    abstract public class GenericValue : IValue
    {
        private DataType _typeImpl;

        public override string ToString()
        {
            return _typeImpl != null ? _typeImpl.ToString() : base.ToString();
        }

        public virtual DataType Type
        {
            get { return _typeImpl; }
            protected set { _typeImpl = value; }
        }

        public virtual double AsNumber()
        {
            throw TypeConversionException.ConvertToNumberException();
        }

        public virtual string AsString()
        {
            return ToString();
        }

        public virtual DateTime AsDate()
        {
            throw TypeConversionException.ConvertToDateException();
        }

        public virtual bool AsBoolean()
        {
            throw TypeConversionException.ConvertToBooleanException();
        }

        public virtual IRuntimeContextInstance AsObject()
        {
            throw TypeConversionException.ConvertToObjectException();
        }

        public virtual bool Equals(IValue other)
        {
            if (other == null)
                return false;

            return this.Equals((object)other);
        }

        public int CompareTo(IValue other)
        {
            if (other == null)
            {
                return _typeImpl == null ? 0 : 1;
            }

            if (_typeImpl != null)
            {
                if (_typeImpl == other.Type)
                {
                    return CompareSameType(other);
                }
                else
                {
                    return _typeImpl.CompareTo(other.Type);
                }
            }
            else
            {
                return -1;
            }
        }

        protected virtual int CompareSameType(IValue other)
        {
            return this.GetHashCode() - other.GetHashCode();
        }
    }
}
