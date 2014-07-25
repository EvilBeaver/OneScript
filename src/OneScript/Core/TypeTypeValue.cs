using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    class TypeTypeValue : IValue
    {

        public TypeTypeValue(DataType type)
        {
            ReferencedType = type;
        }

        public DataType ReferencedType { get; private set; }

        public DataType Type
        {
            get { return BasicTypes.Type; }
        }

        public double AsNumber()
        {
            throw TypeConversionException.ConvertToNumberException();
        }

        public string AsString()
        {
            return ReferencedType.ToString();
        }

        public DateTime AsDate()
        {
            throw TypeConversionException.ConvertToDateException();
        }

        public bool AsBoolean()
        {
            throw TypeConversionException.ConvertToBooleanException();
        }

        public IRuntimeContextInstance AsObject()
        {
            throw TypeConversionException.ConvertToObjectException();
        }

        public bool Equals(IValue other)
        {
            if (other.Type == this.Type)
            {
                var ttv = (TypeTypeValue)other;
                return ttv.ReferencedType == this.ReferencedType;
            }
            else
            {
                return false;
            }
        }

        public int CompareTo(IValue other)
        {
            var ttv = other as TypeTypeValue;
            if (ttv != null)
            {
                return this.ReferencedType.CompareTo(ttv.ReferencedType);
            }
            else
            {
                throw new NotSupportedException("Сравнение допускается только для значений с типом Тип");
            }
        }
    }
}
