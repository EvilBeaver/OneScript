/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace ScriptEngine.Machine.Values
{
    public abstract class GenericValue : IValue
    {
        public virtual int CompareTo(IValue other)
        {
            throw RuntimeException.ComparisonNotSupportedException();
        }

        public virtual bool Equals(IValue other)
        {
            return ReferenceEquals(this, other?.GetRawValue());
        }

        public DataType DataType { get; protected set; }

        public virtual TypeDescriptor SystemType => TypeDescriptor.FromDataType(DataType);

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

        public virtual string AsString() => DataType.ToString();

        public virtual IRuntimeContextInstance AsObject()
        {
            throw RuntimeException.ValueIsNotObjectException();
        }

        public IValue GetRawValue()
        {
            return this;
        }

        public override string ToString()
        {
            return AsString();
        }
    }
}