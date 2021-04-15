/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Globalization;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine.Values;

namespace ScriptEngine.Machine
{
    public static class ValueFactory
    {
        public static IValue Create()
        {
            return BslUndefinedValue.Instance;
        }

        public static IValue Create(string value)
        {
            return BslStringValue.Create(value);
        }

        public static IValue Create(bool value)
        {
            return BslBooleanValue.Create(value);
        }

        public static IValue Create(decimal value)
        {
            return BslNumericValue.Create(value);
        }

        public static IValue Create(int value)
        {
            return BslNumericValue.Create(value);
        }

        public static IValue Create(DateTime value)
        {
            return BslDateValue.Create(value);
        }

        public static IValue CreateInvalidValueMarker()
        {
            return BslSkippedParameterValue.Instance;
        }

        public static IValue CreateNullValue()
        {
            return BslNullValue.Instance;
        }

        public static IValue Parse(string presentation, DataType type)
        {
            IValue result;
            switch (type)
            {
                case DataType.Boolean:
                    result = BslBooleanValue.Parse(presentation);
                    break;
                case DataType.Date:
                    result = BslDateValue.Parse(presentation);
                    break;
                case DataType.Number:
                    result = BslNumericValue.Parse(presentation);
                    break;
                case DataType.String:
                    result = BslStringValue.Create(presentation);
                    break;
                case DataType.Undefined:
                    result = BslUndefinedValue.Instance;
                    break;
                case DataType.GenericValue:
                    if (string.Compare(presentation, "null", StringComparison.OrdinalIgnoreCase) == 0)
                        result = ValueFactory.CreateNullValue();
                    else
                        throw new NotSupportedException("constant type is not supported");

                    break;
                default:
                    throw new NotSupportedException("constant type is not supported");
            }

            return result;
        }

        public static IValue Add(IValue op1, IValue op2)
        {
            // принимаем только RawValue
            Debug.Assert(!(op1 is IVariable || op2 is IVariable));
            
            if (op1 is BslStringValue s)
            {
                return Create(s + op2.AsString());
            }

            if (op1 is BslDateValue date && op2.SystemType == BasicTypes.Number)
            {
                return Create(date + op2.AsNumber());
            }

            // все к числовому типу.
            return Create(op1.AsNumber() + op2.AsNumber());
        }

        public static IValue Sub(IValue op1, IValue op2)
        {
            if (op1 is BslNumericValue n)
            {
                return Create(n - op2.AsNumber());
            }
            if (op1 is BslDateValue date && op2 is BslNumericValue num)
            {
                var result = date - num;
                return Create(result);
            }
            if (op1 is BslDateValue d1 && op2 is BslDateValue d2)
            {
                var diff = d1 - d2;
                return Create(diff);
            }

            // все к числовому типу.
            return Create(op1.AsNumber() - op2.AsNumber());
        }

        public static IValue Mul(IValue op1, IValue op2)
        {
            return Create(op1.AsNumber() * op2.AsNumber());
        }

        public static IValue Div(IValue op1, IValue op2)
        {
            if (op2.AsNumber() == 0)
            {
                throw RuntimeException.DivideByZero();
            }
            return Create(op1.AsNumber() / op2.AsNumber());
        }

        public static IValue Mod(IValue op1, IValue op2)
        {
            if (op2.AsNumber() == 0)
            {
                throw RuntimeException.DivideByZero();
            }
            return Create(op1.AsNumber() % op2.AsNumber());
        }

        public static IValue Neg(IValue op1)
        {
            return Create(op1.AsNumber() * -1);
        }
    }
}
