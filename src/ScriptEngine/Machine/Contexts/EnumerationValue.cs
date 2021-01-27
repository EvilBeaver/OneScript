/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Contexts
{
    abstract public class EnumerationValue : IValue
    {
        readonly EnumerationContext _owner;

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

        public string ValuePresentation
        {
            get;set;
        }

        public virtual DataType DataType
        {
            get { return Machine.DataType.GenericValue; }
        }

        public virtual TypeDescriptor SystemType => _owner.ValuesType;

        public virtual decimal AsNumber()
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
            return ValuePresentation == null ? SystemType.Name : ValuePresentation;
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
                    throw RuntimeException.ComparisonNotSupportedException();
                }
            }
            else
            {
                return 1;
            }
        }

        public virtual bool Equals(IValue other)
        {
            return other.GetRawValue() == this;
        }
    }
}
