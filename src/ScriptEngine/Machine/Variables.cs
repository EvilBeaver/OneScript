/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Core;
using ScriptEngine.Types;

namespace ScriptEngine.Machine
{
    public interface IVariable : IValue
    {
        IValue Value { get; set; }
        string Name { get; }
    }

    public class Variable : IVariable
    {
        public string Name { get; private set; }

        private Variable()
        {

        }

        #region Factory

        public static IVariable Create(IValue val, string symbol)
        {
            return new Variable()
            {
                Value = val,
                Name = symbol
            };
        }

        public static IVariable Create(IValue val, VariableInfo metadata)
        {
            return new Variable()
            {
                Value = val,
                Name = metadata.Identifier
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
        
        public decimal AsNumber()
        {
            return Value.AsNumber();
        }

        public DateTime AsDate()
        {
            return Value.AsDate();
        }

        public bool AsBoolean()
        {
            return Value.AsBoolean();
        }

        public string AsString()
        {
            return Value.AsString();
        }

        public IContext AsContext()
        {
            return Value.AsContext();
        }

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
            private ReferenceType _refType;
            private IVariable _referencedValue;
            private IRuntimeContextInstance _context;
            private int _contextPropertyNumber;
            private IValue _index;

            public string Name { get; private set; }

            public VariableReference(IVariable var, string name)
            {
                _refType = ReferenceType.Simple;
                _referencedValue = var;
                Name = name;
            }

            public VariableReference(IRuntimeContextInstance context, int propertyNumber, string name)
            {
                _refType = ReferenceType.ContextProperty;
                _context = context;
                _contextPropertyNumber = propertyNumber;
                Name = name;
            }

            public VariableReference(IRuntimeContextInstance context, IValue index, string name)
            {
                _refType = ReferenceType.IndexedProperty;
                _context = context;
                _index = index;
                Name = name;
            }

            private enum ReferenceType
            {
                Simple,
                ContextProperty,
                IndexedProperty
            }

            #region IVariable Members

            public IValue Value
            {
                get
                {
                    if (_refType == ReferenceType.Simple)
                    {
                        return _referencedValue.Value;
                    }
                    else if (_refType == ReferenceType.ContextProperty)
                    {
                        if (_context.IsPropReadable(_contextPropertyNumber))
                            return _context.GetPropValue(_contextPropertyNumber);
                        else
                            throw RuntimeException.PropIsNotReadableException("");
                    }
                    else
                    {
                        return _context.GetIndexedValue(_index);
                    }
                }
                set
                {
                    if (_refType == ReferenceType.Simple)
                    {
                        _referencedValue.Value = value;
                    }
                    else if (_refType == ReferenceType.ContextProperty)
                    {
                        if(_context.IsPropWritable(_contextPropertyNumber))
                            _context.SetPropValue(_contextPropertyNumber, value);
                        else
                            throw RuntimeException.PropIsNotWritableException("");
                    }
                    else
                    {
                        _context.SetIndexedValue(_index, value);
                    }
                }
            }

            #endregion

            #region IValue Members

            public TypeDescriptor SystemType => Value.SystemType;

            public decimal AsNumber()
            {
                return Value.AsNumber();
            }

            public DateTime AsDate()
            {
                return Value.AsDate();
            }

            public bool AsBoolean()
            {
                return Value.AsBoolean();
            }

            public string AsString()
            {
                return Value.AsString();
            }

            public IContext AsContext()
            {
                return Value.AsObject();
            }

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
            
        }

        #endregion
    }

}
