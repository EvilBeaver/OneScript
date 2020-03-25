/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Machine.Values
{
    public class TypeTypeValue : GenericValue
    {
        readonly TypeDescriptor _instance;

        public TypeTypeValue(string name)
        {
            _instance = TypeManager.GetTypeByName(name);
            DataType = DataType.Type;
        }

        public TypeTypeValue(TypeDescriptor type)
        {
            _instance = type;
            DataType = DataType.Type;
        }

        public override TypeDescriptor SystemType => TypeDescriptor.FromDataType(DataType.Type);

        public override string AsString()
        {
            return _instance.ToString();
        }

        public override bool Equals(IValue other)
        {
            if(other?.DataType == DataType)
            {
                var otherVal = other.GetRawValue() as TypeTypeValue;
                return otherVal._instance.ID == this._instance.ID;
            }

            return false;
        }

		public override bool Equals(object obj)
		{
			if (obj is TypeTypeValue value)
				return Equals(value);
			return false;
		}

		public override int GetHashCode()
		{
			return _instance.GetHashCode();
		}

        public TypeDescriptor Value => _instance;
    }
}
