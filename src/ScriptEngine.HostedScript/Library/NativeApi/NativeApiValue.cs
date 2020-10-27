using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    abstract class NativeApiValue: IValue
    {
        private TypeDescriptor _type;

        #region IValue Members

        public DataType DataType
        {
            get { return Machine.DataType.Object; }
        }

        public TypeDescriptor SystemType
        {
            get
            {
                if (_type.Name == null)
                {
                    if (TypeManager.IsKnownType(this.GetType()))
                    {
                        _type = TypeManager.GetTypeByFrameworkType(this.GetType());
                    }
                    else
                    {
                        throw new InvalidOperationException($"Type {GetType()} is not defined");
                    }
                }

                return _type;
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

        public virtual string AsString()
        {
            return SystemType.Name;
        }

        public IValue GetRawValue()
        {
            return this;
        }

        public abstract IRuntimeContextInstance AsObject();

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

        public virtual bool Equals(IValue other)
        {
            if (other == null)
                return false;

            return other.SystemType.Equals(this.SystemType) && Object.ReferenceEquals(this.AsObject(), other.AsObject());
        }

        #endregion
    }
}