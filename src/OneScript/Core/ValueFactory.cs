using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public static class ValueFactory
    {
        public static IValue Create()
        {
            return UndefinedValue.Instance;
        }

        public static IValue Create(string value)
        {
            return new StringValue(value);
        }

        public static IValue Create(bool value)
        {
            return new BooleanValue(value);
        }

        public static IValue Create(double value)
        {
            return new NumericValue((decimal)value);
        }

        public static IValue Create(int value)
        {
            return new NumericValue((int)value);
        }

        public static IValue Create(decimal value)
        {
            return new NumericValue(value);
        }

        public static IValue Create(DateTime value)
        {
            return new DateValue(value);
        }

        public static IValue Create(DataType value)
        {
            return new TypeTypeValue(value);
        }

        public static IValue Parse(string presentation, DataType type)
        {
            IValue result;
            if (type == BasicTypes.Boolean)
            {
                result = ParseAsBoolean(presentation);
            }
            else if (type == BasicTypes.Date)
            {
                result = ParseAsDate(presentation);
            }
            else if (type == BasicTypes.Number)
            {
                result = ParseAsNumber(presentation);
            }
            else if (type == BasicTypes.String)
            {
                result = ValueFactory.Create(presentation);
            }
            else if (type == BasicTypes.Undefined)
            {
                result = ValueFactory.Create();
            }
            else
                throw new NotImplementedException("constant type is not supported");

            return result;
        }

        private static IValue ParseAsNumber(string presentation)
        {
            IValue result;
            var numInfo = System.Globalization.NumberFormatInfo.InvariantInfo;
            var numStyle = System.Globalization.NumberStyles.AllowDecimalPoint
                        | System.Globalization.NumberStyles.AllowLeadingSign;

            try
            {
                result = ValueFactory.Create(Double.Parse(presentation, numStyle, numInfo));
            }
            catch (FormatException)
            {
                throw TypeConversionException.ConvertToNumberException();
            }
            return result;
        }

        private static IValue ParseAsDate(string presentation)
        {
            IValue result;
            string format;
            if (presentation.Length == 8)
                format = "yyyyMMdd";
            else if (presentation.Length == 14)
                format = "yyyyMMddHHmmss";
            else
                throw TypeConversionException.ConvertToDateException();

            try
            {
                result = ValueFactory.Create(DateTime.ParseExact(presentation, format, System.Globalization.CultureInfo.InvariantCulture));
            }
            catch (FormatException)
            {
                throw TypeConversionException.ConvertToDateException();
            }
            return result;
        }

        private static IValue ParseAsBoolean(string presentation)
        {
            IValue result;
            if (string.Compare(presentation, "истина", true) == 0)
                result = ValueFactory.Create(true);
            else if (string.Compare(presentation, "ложь", true) == 0)
                result = ValueFactory.Create(false);
            else
                throw TypeConversionException.ConvertToBooleanException();
            return result;
        }

    }
}
