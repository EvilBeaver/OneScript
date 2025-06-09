/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Dynamic;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Types;
using ScriptEngine.Machine;

namespace OneScript.Values
{
    public abstract class BslValue : DynamicObject, IComparable<BslValue>, IEquatable<BslValue>, IValue
    {
        /// <summary>
        /// Bsl-представление объекта. Имеет смысл переопределять, если представление при вызове
        /// из BSL должно отличаться от стандартного метода ToString, либо, если формирование
        /// представления требует выполнения Bsl-кода.
        /// </summary>
        /// <param name="process">Текущий процесс, в котором вызван данный метод</param>
        public virtual string ToString(IBslProcess process)
        {
            return ToString();
        }

        public abstract int CompareTo(BslValue other);

        public abstract bool Equals(BslValue other);
        
        public static explicit operator bool(BslValue target) =>
            target switch
            {
                BslBooleanValue v => (bool) v,
                BslNumericValue nv => (bool) nv,
                BslStringValue sv => (bool) sv,
                _ => throw BslExceptions.ConvertToBooleanException()
            };

        public static explicit operator decimal(BslValue target) =>
            target switch
            {
                BslNumericValue v => (decimal) v,
                BslStringValue sv => (decimal) sv,
                BslBooleanValue bv => (decimal) bv,
                _ => throw BslExceptions.ConvertToNumberException(target)
            };

        public static explicit operator int(BslValue target) =>
            target switch
            {
                BslNumericValue v => (int) (decimal) v,
                BslStringValue sv => (int) (decimal) sv,
                BslBooleanValue bv => (int) (decimal) bv,
                _ => throw BslExceptions.ConvertToNumberException()
            };

        public static explicit operator DateTime(BslValue target) =>
            target switch
            {
                BslDateValue v => (DateTime) v,
                BslStringValue sv => (DateTime) sv,
                _ => throw BslExceptions.ConvertToDateException()
            };

        #region Stack Runtime Bridge

        public virtual TypeDescriptor SystemType => BasicTypes.UnknownType;

        public int CompareTo(IValue other) => CompareTo(UnwrapReference(other));

        public bool Equals(IValue other) => Equals(UnwrapReference(other));

        public virtual IValue GetRawValue() => this;

        private BslValue UnwrapReference(IValue v)
        {
            if (v is IValueReference r)
                return r.BslValue;

            return (BslValue)v;
        }
        
        #endregion
    }
}
