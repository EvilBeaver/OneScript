using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("УникальныйИдентификатор","UUID")]
    public class GuidWrapper : IValue
    {
        Guid _value;

        public GuidWrapper()
        {
            _value = Guid.NewGuid();
        }

        public GuidWrapper(string uuidString)
        {
            _value = Guid.Parse(uuidString);
        }

        [ScriptConstructor]
        public static GuidWrapper Create()
        {
            return new GuidWrapper();
        }

        [ScriptConstructor]
        public static GuidWrapper Create(IValue uuidString)
        {
            return new GuidWrapper(uuidString.AsString());
        }

        public DataType DataType
        {
            get { return Machine.DataType.GenericValue; }
        }

        public TypeDescriptor SystemType
        {
            get
            {
                return TypeManager.GetTypeByFrameworkType(typeof(GuidWrapper));
            }
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
            return _value.ToString();
        }

        public TypeDescriptor AsType()
        {
            throw new NotImplementedException();
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
            GuidWrapper otherUuid = other.GetRawValue() as GuidWrapper;
            if (otherUuid == null)
                throw RuntimeException.ComparisonNotSupportedException();

            return _value.CompareTo(otherUuid._value);
        }

        public bool Equals(IValue other)
        {
            GuidWrapper otherUuid = other.GetRawValue() as GuidWrapper;
            if (otherUuid == null)
                return false;
            else
                return _value.Equals(otherUuid._value);
        }

        public int AddRef()
        {
            return 1;
        }

        public int Release()
        {
            return 1;
        }
    }
}
