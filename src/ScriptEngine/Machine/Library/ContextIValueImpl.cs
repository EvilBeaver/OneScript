using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    abstract class ContextIValueImpl : IRuntimeContextInstance, IValue
    {
        private TypeDescriptor _type;

        public ContextIValueImpl()
        {
            if (!StdLib.IsKnownType(this.GetType(), out _type))
            {
                throw new InvalidOperationException("Type is not defined");
            }
        }

        public ContextIValueImpl(TypeDescriptor type)
        {
            DefineType(type);
        }

        protected void DefineType(TypeDescriptor type)
        {
            _type = type;
        }

        public override string ToString()
        {
            return _type.Name;
        }
        
        #region IValue Members

        public DataType DataType
        {
            get { return Machine.DataType.Object; }
        }

        public TypeDescriptor SystemType
        {
            get { return _type; }
        }

        public double AsNumber()
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
            return ToString();
        }

        public TypeDescriptor AsType()
        {
            throw new NotImplementedException();
        }

        public IRuntimeContextInstance AsObject()
        {
            return this;
        }

        public IValue GetRawValue()
        {
            return this;
        }

        #endregion

        #region IComparable<IValue> Members

        public int CompareTo(IValue other)
        {
            if (other.SystemType.Equals(this.SystemType))
            {
                if (this.Equals(other))
                {
                    return 0;
                }
                else
                {
                    throw RuntimeException.ComparisonNotSupportedException();
                }
            }
            else
            {
                return this.SystemType.ToString().CompareTo(other.SystemType.ToString());
            }
        }

        #endregion

        #region IEquatable<IValue> Members

        public bool Equals(IValue other)
        {
            if (other.SystemType.Equals(this.SystemType))
            {
                return Object.ReferenceEquals(this.AsObject(), other.AsObject());
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IRuntimeContextInstance Members

        public virtual bool IsIndexed
        {
            get { return false; }
        }
        public virtual IValue GetIndexedValue(IValue index)
        {
            throw new NotImplementedException();
        }

        public virtual void SetIndexedValue(IValue index, IValue val)
        {
            throw new NotImplementedException();
        }
        public virtual int FindProperty(string name)
        {
            throw RuntimeException.PropNotFoundException(name);
        }
        public virtual bool IsPropReadable(int propNum)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsPropWritable(int propNum)
        {
            throw new NotImplementedException();
        }
        public virtual IValue GetPropValue(int propNum)
        {
            throw new NotImplementedException();
        }
        public virtual void SetPropValue(int propNum, IValue newVal)
        {
            throw new NotImplementedException();
        }
        public virtual int FindMethod(string name)
        {
            throw RuntimeException.MethodNotFoundException(name);
        }
        public virtual MethodInfo GetMethodInfo(int methodNumber)
        {
            throw new NotImplementedException();
        }
        public virtual void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            throw new NotImplementedException();
        }
        public virtual void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Method)]
    class ScriptConstructorAttribute : Attribute
    {
        public bool ParametrizeWithClassName { get; set; }
    }
}
