/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;
using OneScript.Types;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Абстрактный класс, реализующий типовые методы интерфейса IValue
    /// </summary>
    public abstract class NativeApiValue: IValue
    {
        #region IValue Members

        protected void DefineType(TypeDescriptor type)
        {
            SystemType = type;
        }

        public TypeDescriptor SystemType { get; private set; }

        public decimal AsNumber()
        {
            throw BslExceptions.ConvertToNumberException();
        }

        public DateTime AsDate()
        {
            throw BslExceptions.ConvertToDateException();
        }

        public bool AsBoolean()
        {
            throw BslExceptions.ConvertToBooleanException();
        }

        public override string ToString()
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