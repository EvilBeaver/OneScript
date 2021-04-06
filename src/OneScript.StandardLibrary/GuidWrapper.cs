/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Values;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary
{
    [ContextClass("УникальныйИдентификатор","UUID", TypeUUID = "B35D7F7B-DF1C-4D6C-A755-6C97A60BB345")]
    public class GuidWrapper : GenericValue, IObjectWrapper
    {
        Guid _value;
        private static readonly TypeDescriptor InstanceType;

        static GuidWrapper()
        {
            var attr = typeof(GuidWrapper).GetCustomAttribute<ContextClassAttribute>();
            InstanceType = typeof(GuidWrapper).GetTypeFromClassMarkup();
        }
        
        public GuidWrapper()
        {
            _value = Guid.NewGuid();
            DataType = DataType.GenericValue;
        }

        public GuidWrapper(string uuidString)
        {
            _value = Guid.Parse(uuidString);
            DataType = DataType.GenericValue;
        }

        [ScriptConstructor]
        public static GuidWrapper Create()
        {
            return new GuidWrapper();
        }

        [ScriptConstructor(Name = "Из строки")]
        public static GuidWrapper Create(IValue uuidString)
        {
            return new GuidWrapper(uuidString.AsString());
        }
        
        public override TypeDescriptor SystemType => InstanceType;

        public override string AsString()
        {
            return _value.ToString();
        }

        public override int CompareTo(IValue other)
        {
            GuidWrapper otherUuid = other.GetRawValue() as GuidWrapper;
            if (otherUuid == null)
                throw RuntimeException.ComparisonNotSupportedException();

            return _value.CompareTo(otherUuid._value);
        }

        public override bool Equals(IValue other)
        {
            GuidWrapper otherUuid = other.GetRawValue() as GuidWrapper;
            if (otherUuid == null)
                return false;
            else
                return _value.Equals(otherUuid._value);
        }

        public override bool IsEmpty => _value.Equals(Guid.Empty);

        object IObjectWrapper.UnderlyingObject => _value;
    }
}
