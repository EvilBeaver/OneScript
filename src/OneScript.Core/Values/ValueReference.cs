using System;
using ScriptEngine.Machine;

namespace OneScript.Values
{
    public class ValueReference : IValueReference
    {
        private readonly Func<IValue> _getter;
        private readonly Action<IValue> _setter;

        public ValueReference(Func<IValue> getter, Action<IValue> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public IValue Value
        {
            get => _getter();
            set => _setter(value);
        }
        
        public bool Equals(IValueReference other)
        {
            return ReferenceEquals(this, other);
        }
    }
}