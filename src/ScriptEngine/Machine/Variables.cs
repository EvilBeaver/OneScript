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
    public interface IVariable : IValue
    {
        IValue Value { get; set; }
        string Name { get; }
    }

    public class Variable : IVariable
    {
        private IValue _val;
        
        public string Name { get; private set; }

        private Variable()
        {

        }

        #region Factory

        public static IVariable Create(IValue val, string symbol)
        {
            return new Variable()
            {
                _val = val,
                Name = symbol
            };
        }

        public static IVariable Create(IValue val, VariableInfo metadata)
        {
            return new Variable()
            {
                _val = val,
                Name = metadata.Identifier
            };
        }

        public static IVariable CreateReference(IVariable variable, string refName)
        {
            return VariableReference.CreateSimpleReference(variable, refName);
        }

        public static IVariable CreateContextPropertyReference(IRuntimeContextInstance context, int propertyNumber, string refName)
        {
            return VariableReference.CreateContextPropertyReference(context, propertyNumber, refName);
        }

        public static IVariable CreateIndexedPropertyReference(IRuntimeContextInstance context, IValue index, string refName)
        {
            return VariableReference.CreateIndexedPropertyReference(context, index, refName);
        }

        #endregion

        #region IVariable Members

        public IValue Value
        {
            get
            {
                return _val;
            }
            set
            {
                _val = value;
            }
        }

        #endregion

        #region IValue Members

        public DataType DataType
        {
            get { return Value.DataType; }
        }

        public TypeDescriptor SystemType
        {
            get { return Value.SystemType; }
        }

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

        public IRuntimeContextInstance AsObject()
        {
            return Value.AsObject();
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

            private VariableReference()
            {

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

            public DataType DataType
            {
                get { return Value.DataType; }
            }

            public TypeDescriptor SystemType
            {
                get { return Value.SystemType; }
            }

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

            public IRuntimeContextInstance AsObject()
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

            public static IVariable CreateSimpleReference(IVariable var, string name)
            {
                var newVar = new VariableReference();
                newVar._refType = ReferenceType.Simple;
                newVar._referencedValue = var;
                newVar.Name = name;
                return newVar;
            }

            public static IVariable CreateContextPropertyReference(IRuntimeContextInstance context, int propertyNumber, string name)
            {
                var newVar = new VariableReference();
                newVar._refType = ReferenceType.ContextProperty;
                newVar._context = context;
                newVar._contextPropertyNumber = propertyNumber;
                newVar.Name = name;
                return newVar;
            }

            public static IVariable CreateIndexedPropertyReference(IRuntimeContextInstance context, IValue index, string name)
            {
                var newVar = new VariableReference();
                newVar._refType = ReferenceType.IndexedProperty;
                newVar._context = context;
                newVar._index = index;
                newVar.Name = name;
                return newVar;
            }

        }

        #endregion
    }

}
