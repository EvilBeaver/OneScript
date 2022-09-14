using OneScript.Contexts;
using OneScript.Types;
using OneScript.Values;

namespace ScriptEngine.Machine
{
    public class Variable : IVariable
    {
        public string Name { get; private set; }

        private Variable()
        {

        }

        #region Factory

        public static IVariable Create(IValue val, string symbol)
        {
            return new Variable
            {
                Value = val,
                Name = symbol
            };
        }

        public static IVariable CreateReference(IVariable variable, string refName)
        {
            return new VariableReference(variable, refName);
        }

        public static IVariable CreateContextPropertyReference(IRuntimeContextInstance context, int propertyNumber, string refName)
        {
            return new VariableReference(context, propertyNumber, refName);
        }

        public static IVariable CreateIndexedPropertyReference(IRuntimeContextInstance context, IValue index, string refName)
        {
            return new VariableReference(context, index, refName);
        }

        #endregion

        #region IVariable Members

        public IValue Value { get; set; }

        #endregion

        #region IValue Members

        public TypeDescriptor SystemType => Value.SystemType;

        public IValue GetRawValue()
        {
            return Value;
        }

        #endregion

        #region IComparable<IValue> Members

        public int CompareTo(IValue other)
        {
            return Value.CompareTo(other);
        }

        #endregion

        #region IEquatable<IValue> Members

        public bool Equals(IValue other)
        {
            return Value.Equals(other);
        }

        #endregion

        #region Reference

        private class VariableReference : IVariable
        {
            private readonly IValueReference _reference;

            public string Name { get; }

            public VariableReference(IVariable var, string name)
            {
                _reference = new ValueReference(() => (BslValue)var.Value, v => var.Value = v);
                Name = name;
            }

            public VariableReference(IRuntimeContextInstance context, int propertyNumber, string name)
            {
                _reference = new PropertyValueReference(context, propertyNumber);
                Name = name;
            }

            public VariableReference(IRuntimeContextInstance context, IValue index, string name)
            {
                _reference = new IndexedValueReference(context, (BslValue)index);
                Name = name;
            }

            #region IVariable Members

            public IValue Value
            {
                get => _reference.Value;
                set => _reference.Value = (BslValue)value;
            }

            #endregion

            #region IValue Members

            public TypeDescriptor SystemType => Value.SystemType;

            public IValue GetRawValue()
            {
                return Value.GetRawValue();
            }

            #endregion

            #region IComparable<IValue> Members

            public int CompareTo(IValue other)
            {
                return Value.CompareTo(other);
            }

            #endregion

            #region IEquatable<IValue> Members

            public bool Equals(IValue other)
            {
                return Value.Equals(other);
            }

            #endregion

            public bool Equals(IValueReference other)
            {
                return _reference.Equals(other);
            }
        }

        #endregion

        public bool Equals(IValueReference other)
        {
            return ReferenceEquals(this, other);
        }
    }
}