/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Абстрактный класс, реализующий типовые методы интерфейса IValue
    /// </summary>
    public abstract class NativeApiValue: IValue
    {
        private TypeDescriptor _type;

        #region IValue Members

        public DataType DataType
        {
            get { return ScriptEngine.Machine.DataType.Object; }
        }

        protected void DefineType(TypeDescriptor type)
        {
            _type = type;
        }

        public TypeDescriptor SystemType
        {
            get => _type;
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