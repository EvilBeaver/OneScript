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

                    if (String.Compare(presentation, "истина", StringComparison.OrdinalIgnoreCase) == 0 
                        || String.Compare(presentation, "true", StringComparison.OrdinalIgnoreCase) == 0 
                        || String.Compare(presentation, "да", StringComparison.OrdinalIgnoreCase) == 0)
                        result = ValueFactory.Create(true);
                    else if (String.Compare(presentation, "ложь", StringComparison.OrdinalIgnoreCase) == 0 
                             || String.Compare(presentation, "false", StringComparison.OrdinalIgnoreCase) == 0
                             || String.Compare(presentation, "нет", StringComparison.OrdinalIgnoreCase) == 0)
                        result = ValueFactory.Create(false);
                    else
                        throw RuntimeException.ConvertToBooleanException();

                    break;
                case DataType.Date:
                    string format;
                    if (presentation.Length == 14)
                        format = "yyyyMMddHHmmss";
                    else if (presentation.Length == 8)
                        format = "yyyyMMdd";
                    else if (presentation.Length == 12)
                        format = "yyyyMMddHHmm";
                    else
                        throw RuntimeException.ConvertToDateException();

                    if (presentation == "00000000"
                     || presentation == "000000000000"
                     || presentation == "00000000000000")
                    {
                        result = ValueFactory.Create(new DateTime());
                    }
                    else
                        try
                        {
                            result = ValueFactory.Create(DateTime.ParseExact(presentation, format, System.Globalization.CultureInfo.InvariantCulture));
                        }
                        catch (FormatException)
                        {
                            throw RuntimeException.ConvertToDateException();
                        }

                    break;
                case DataType.Number:
                    var numInfo = NumberFormatInfo.InvariantInfo;
                    var numStyle = NumberStyles.AllowDecimalPoint
                                |NumberStyles.AllowLeadingSign
                                |NumberStyles.AllowLeadingWhite
                                |NumberStyles.AllowTrailingWhite;

                    try
                    {
                        result = ValueFactory.Create(Decimal.Parse(presentation, numStyle, numInfo));
                    }
                    catch (FormatException)
                    {
                        throw RuntimeException.ConvertToNumberException();
                    }
                    break;
                case DataType.String:
                    result = ValueFactory.Create(presentation);
                    break;
                case DataType.Undefined:
                    result = ValueFactory.Create();
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
                return Create(date + op1.AsNumber());
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
