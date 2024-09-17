﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Contexts
{
    public sealed class Variable : IVariable
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
            if (variable is VariableReference vref)
            {
                if (vref._reference is IndexedValueReference iv)
                {
                    _ = iv.Value;
                }

                return variable;
            }
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

        private sealed class VariableReference : IVariable
        {
            public readonly IValueReference _reference;

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

        public override string ToString()
        {
            return Name;
        }
    }
}
