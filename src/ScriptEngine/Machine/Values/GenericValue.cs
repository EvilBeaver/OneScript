/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Values
{
    public abstract class GenericValue : BslPrimitiveValue, IValue, IEmptyValueCheck
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

        public virtual TypeDescriptor SystemType => FromDataType(DataType);

        private static TypeDescriptor FromDataType(DataType srcType)
        {
            switch (srcType)
            {
                case DataType.Boolean:
                    return BasicTypes.Boolean;
                case DataType.Date:
                    return BasicTypes.Date;
                case DataType.Number:
                    return BasicTypes.Number;
                case DataType.String:
                    return BasicTypes.String;
                case DataType.Undefined:
                    return BasicTypes.Undefined;
                case DataType.Type:
                    return BasicTypes.Type;
                default:
                    Debug.Assert(false, "Can be used only for primitive types");
                    return default;
            }
        }
        
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

        public virtual bool IsEmpty => false;
    }
}