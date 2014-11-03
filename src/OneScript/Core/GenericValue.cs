using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    abstract public class GenericValue : IValue
    {
        public virtual DataType Type
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string AsString()
        {
            return ToString();
        }

        public virtual DateTime AsDate()
        {
            throw TypeConversionException.ConvertToDateException();
        }

        public virtual decimal AsNumber()
        {
            throw TypeConversionException.ConvertToNumberException();
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
                return 1;
            }

            if (this.Type == other.Type)
            {
                return CompareSameType(other);
            }
            else
            {
                throw TypeConversionException.ComparisonIsNotSupportedException();
            }
        }

        protected virtual int CompareSameType(IValue other)
        {
            return this.GetHashCode() - other.GetHashCode();
        }

        public virtual int AddRef()
        {
            return 1;
        }

        public virtual int Release()
        {
            return 1;
        }
    }
}
