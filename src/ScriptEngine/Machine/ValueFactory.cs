using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public static class ValueFactory
    {
        public static IValue Create()
        {
            return SimpleConstantValue.Undefined();
        }

        public static IValue Create(string value)
        {
            return new StringConstantValue(value);
        }

        public static IValue Create(bool value)
        {
            return SimpleConstantValue.Boolean(value);
        }

        public static IValue Create(double value)
        {
            return SimpleConstantValue.Number(value);
        }

        public static IValue Create(DateTime value)
        {
            return SimpleConstantValue.DateTime(value);
        }

        public static IValue CreateInvalidValueMarker()
        {
            return new InvalidValue();
        }

        public static IValue Create(IRuntimeContextInstance instance)
        {
            return (IValue)instance;
        }

        public static IValue Parse(string presentation, DataType type)
        {
            IValue result;
            switch (type)
            {
                case DataType.Boolean:

                    if (string.Compare(presentation, "истина", true) == 0)
                        result = ValueFactory.Create(true);
                    else if (string.Compare(presentation, "ложь", true) == 0)
                        result = ValueFactory.Create(false);
                    else
                        throw RuntimeException.ConvertToBooleanException();

                    break;
                case DataType.Date:
                    string format;
                    if (presentation.Length == 8)
                        format = "yyyyMMdd";
                    else if (presentation.Length == 14)
                        format = "yyyyMMddHHmmss";
                    else
                        throw RuntimeException.ConvertToDateException();

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
                    var numInfo = System.Globalization.NumberFormatInfo.InvariantInfo;
                    var numStyle = System.Globalization.NumberStyles.AllowDecimalPoint
                                |System.Globalization.NumberStyles.AllowLeadingSign;

                    try
                    {
                        result = ValueFactory.Create(Double.Parse(presentation, numStyle, numInfo));
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
                default:
                    throw new NotImplementedException("constant type is not supported");
            }

            return result;
        }

        class InvalidValue : IValue
        {

            #region IValue Members

            public DataType DataType
            {
                get { return Machine.DataType.NotAValidValue; }
            }

            public TypeDescriptor SystemType
            {
                get { throw new NotImplementedException(); }
            }

            public double AsNumber()
            {
                throw new NotImplementedException();
            }

            public DateTime AsDate()
            {
                throw new NotImplementedException();
            }

            public bool AsBoolean()
            {
                throw new NotImplementedException();
            }

            public string AsString()
            {
                throw new NotImplementedException();
            }

            public TypeDescriptor AsType()
            {
                throw new NotImplementedException();
            }

            public IRuntimeContextInstance AsObject()
            {
                throw new NotImplementedException();
            }

            public IValue GetRawValue()
            {
                return this;
            }

            #endregion

            #region IComparable<IValue> Members

            public int CompareTo(IValue other)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEquatable<IValue> Members

            public bool Equals(IValue other)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

    }
}
