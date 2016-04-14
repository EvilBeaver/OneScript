using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class InjectedVariable : IValueRef, IValue
    {
        public InjectedVariable()
        {

        }

        public InjectedVariable(string name, IValue value)
        {
            Name = name;
            Value = value;
        }

        public IValue Value { get; set; }
        
        public string Name { get; set; }

        public DataType Type
        {
            get
            {
                return Value.Type;
            }
        }
        
        public bool AsBoolean()
        {
            return Value.AsBoolean();
        }

        public DateTime AsDate()
        {
            return Value.AsDate();
        }

        public decimal AsNumber()
        {
            return Value.AsNumber();
        }

        public IRuntimeContextInstance AsObject()
        {
            return Value.AsObject();
        }

        public string AsString()
        {
            return Value.AsString();
        }

        public int CompareTo(IValue other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(IValue other)
        {
            return Value.Equals(other);
        }

        public int AddRef()
        {
            return Value.AddRef();
        }

        public int Release()
        {
            return Value.Release();
        }
    }
}
