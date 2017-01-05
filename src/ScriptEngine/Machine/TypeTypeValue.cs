/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public class TypeTypeValue : IValue
    {
        readonly TypeDescriptor _instance;

        public TypeTypeValue(string name)
        {
            _instance = TypeManager.GetTypeByName(name);
        }

        public TypeTypeValue(TypeDescriptor type)
        {
            _instance = type;
        }

        public DataType DataType
        {
            get { return Machine.DataType.Type; }
        }

        public TypeDescriptor SystemType
        {
            get { return TypeDescriptor.FromDataType(DataType.Type); }
        }

        public decimal AsNumber()
        {
            throw RuntimeException.ConvertToNumberException();
        }

        public DateTime AsDate()
        {
            throw RuntimeException.ConvertToDateException();
        }

        public bool AsBoolean()
        {
            throw RuntimeException.ConvertToBooleanException();
        }

        public string AsString()
        {
            return _instance.ToString();
        }

        public IRuntimeContextInstance AsObject()
        {
            throw RuntimeException.ValueIsNotObjectException();
        }

        public IValue GetRawValue()
        {
            return this;
        }

        public int CompareTo(IValue other)
        {
            throw RuntimeException.ComparisonNotSupportedException();
        }

        public bool Equals(IValue other)
        {
            if(other.DataType == this.DataType)
            {
                var otherVal = other.GetRawValue() as TypeTypeValue;
                return otherVal._instance.ID == this._instance.ID;
            }
            else
            {
                return false;
            }
        }

		public override bool Equals(object obj)
		{
			if (obj is TypeTypeValue)
				return Equals(obj as TypeTypeValue);
			return false;
		}

		public override int GetHashCode()
		{
			return _instance.GetHashCode();
		}

        public TypeDescriptor Value
        {
            get
            {
                return _instance;
            }
        }

    }
}
