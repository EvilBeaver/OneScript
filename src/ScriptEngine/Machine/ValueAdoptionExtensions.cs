/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    public static class ValueAdoptionExtensions
    {
        public static bool AsBoolean(this BslValue val) => (bool) val;
        public static DateTime AsDate(this BslValue val) => (DateTime) val;
        public static decimal AsNumber(this BslValue val) => (decimal) val;
        
        public static IRuntimeContextInstance AsObject(this BslValue val) 
            => val is IRuntimeContextInstance ctx? ctx : throw BslExceptions.ValueIsNotObjectException();
        
        public static bool AsBoolean(this IValue val) => (bool) UnwrapReference(val);
        public static DateTime AsDate(this IValue val) => (DateTime) UnwrapReference(val);
        public static decimal AsNumber(this IValue val) => (decimal) UnwrapReference(val);
        
        // Метод для совместимости внешних компонент
        [Obsolete("Use overload with IBslProcess")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AsString(this IValue val) => AsString(val, ForbiddenBslProcess.Instance);
        
        public static string AsString(this IValue val, IBslProcess process) => ((BslValue)val).ToString(process);

        // Метод для совместимости внешних компонент
        [Obsolete("Just don't use. IValue is always marshalled as raw value.")]
        public static IValue GetRawValue(this IValue val)
        {
            return UnwrapReference(val);
        }
        
        public static string ExplicitString(this IValue val)
        {
            if (val == null)
                return "";
            
            if (val.SystemType == BasicTypes.String)
                return UnwrapReference(val).ToString();

            throw RuntimeException.InvalidArgumentType();
        }
        
        public static IRuntimeContextInstance AsObject(this IValue val) 
            => UnwrapReference(val) as IRuntimeContextInstance ?? throw BslExceptions.ValueIsNotObjectException();

        public static object UnwrapToClrObject(this IValue value)
        {
            return ContextValuesMarshaller.ConvertToClrObject(value);
        }

        public static bool IsSkippedArgument(this IValue val)
        {
            return ReferenceEquals(val, BslSkippedParameterValue.Instance);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BslValue UnwrapReference(IValue v)
        {
            if (v is IValueReference r)
                return r.BslValue;

            return (BslValue)v;
        }
    }
}