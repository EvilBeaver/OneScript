using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace ScriptEngine.Machine
{
    static class ValueFormatter
    {
        static readonly string[] BOOLEAN_FALSE = { "БЛ", "BT" };
        static readonly string[] BOOLEAN_TRUE = { "БИ", "BF" };
        static readonly string[] LOCALE = { "Л", "L" };
        static readonly string[] NUM_MAX_SIZE = { "ЧЦ", "ND" };
        static readonly string[] NUM_DECIMAL_SIZE = { "ЧДЦ", "NFD" };
        static readonly string[] NUM_FRACTION_DELIMITER = { "ЧРД", "NDS" };
        static readonly string[] NUM_GROUPS_DELIMITER = { "ЧРГ", "NGS" };
        static readonly string[] NUM_ZERO_APPEARANCE = { "ЧН", "NZ" };
        static readonly string[] NUM_GROUPING = { "ЧГ", "NG" };
        static readonly string[] NUM_LEADING_ZERO = { "ЧВН", "NLZ" };

        // Длины разрядов мантиссы типа decimal в строковом десятичном представлении
        static readonly int[] decimal_digits_sizes = { 10, 20, 29 };

        private struct FormatParameter
        {
            private string _name;
            private string _value;

            public FormatParameter(string name, string value)
            {
                _name = name;
                _value = value;
            }

            public string Name
            {
                get { return _name; }
            }

            public string Value
            {
                get { return _value; }
            }


        }
        
        #region Format string parser

        private class FormatParametersList
        {
            List<FormatParameter> _paramList = new List<FormatParameter>();

            public FormatParametersList(string format)
            {
                ParseParameters(format);
            }
            private void ParseParameters(string format)
            {
                int index = 0;
                int len = format.Length;
                while (index < len)
                {
                    SkipWhitespace(format, ref index);

                    string param = ReadParameter(format, ref index);
                    if (param == null)
                        break;

                    SkipWhitespace(format, ref index);

                    string value = ReadValue(format, ref index);
                    if (value == null)
                        value = "";

                    _paramList.Add(new FormatParameter(param, value));
                }

            }

            private static void SkipWhitespace(string format, ref int index)
            {
                while (index < format.Length)
                {
                    if (Char.IsWhiteSpace(format, index))
                        index++;
                    else
                        break;
                }
            }

            private static string ReadParameter(string format, ref int index)
            {
                if (!Char.IsLetter(format, index))
                    return null;

                int start = index;
                while (index < format.Length)
                {
                    if (Char.IsLetter(format, index))
                        index++;
                    else if (format[index] == '=')
                    {
                        var param = format.Substring(start, GetLength(start, index));
                        index++;
                        return param.Trim();
                    }
                    else if (Char.IsWhiteSpace(format, index))
                    {
                        SkipWhitespace(format, ref index);
                    }
                    else
                        return null;

                }

                return null;
            }

            private static string ReadValue(string format, ref int index)
            {
                const char SINGLE_QUOTE = '\'';
                const char DOUBLE_QUOTE = '\"';
                const char SPACE = ' ';

                if (index >= format.Length)
                    return null;

                StringBuilder valueBuffer = new StringBuilder();
                char valueEnd = '\0';

                if (format[index] == SINGLE_QUOTE)
                {
                    valueEnd = SINGLE_QUOTE;
                    index++;
                }
                else if (format[index] == DOUBLE_QUOTE)
                {
                    valueEnd = DOUBLE_QUOTE;
                    index++;
                }
                else
                {
                    valueEnd = SPACE;
                }

                int start = index;

                while (index < format.Length)
                {
                    if (format[index] == valueEnd)
                    {
                        if (index + 1 < format.Length && format[index + 1] == valueEnd)
                        {
                            index += 2;
                            valueBuffer.Append(valueEnd);
                            continue;
                        }
                        break;
                    }
                    else if (valueEnd == SPACE && format[index] == ';')
                    {
                        break;
                    }
                    else
                        valueBuffer.Append(format[index++]);
                }

                if (index < format.Length)
                {
                    if (valueEnd == DOUBLE_QUOTE || valueEnd == SINGLE_QUOTE)
                    {
                        index++;
                        SkipWhitespace(format, ref index);
                    }
                    else
                    {
                        SkipWhitespace(format, ref index);
                    }

                    if (index < format.Length && format[index] == ';')
                        index++;
                }

                return valueBuffer.ToString();

            }

            private static int GetLength(int start, int index)
            {
                if (index > start)
                    return index - start;
                else if (index == start)
                    return 1;
                else
                    return 0;
            }

            public string GetParamValue(string paramName)
            {
                return FindParamValue(x => String.Compare(x.Name, paramName, true) == 0);
            }

            public string GetParamValue(string paramNameRus, string paramNameEng)
            {
                return FindParamValue(x => String.Compare(x.Name, paramNameRus, true) == 0
                    || String.Compare(x.Name, paramNameEng, true) == 0);

            }

            public string GetParamValue(string[] biLingua)
            {
                return GetParamValue(biLingua[0], biLingua[1]);

            }

            public bool HasParam(string[] biLingua, out string value)
            {
                var paramValue = GetParamValue(biLingua);
                if (paramValue != null)
                {
                    value = paramValue;
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }

            }

            private string FindParamValue(Predicate<FormatParameter> criteria)
            {
                var param = _paramList.Find(criteria);
                if (param.Name == null)
                    return null;

                return param.Value;
            }

        }

        #endregion

        public static string Format(IValue value, string format)
        {
            var formatParameters = ParseParameters(format);

            string formattedValue;

            switch(value.DataType)
            {
                case DataType.Boolean:
                    formattedValue = FormatBoolean(value.AsBoolean(), formatParameters);
                    break;
                case DataType.Number:
                    formattedValue = FormatNumber(value.AsNumber(), formatParameters);
                    break;
                case DataType.Date:
                    formattedValue = FormatDate(value.AsDate(), formatParameters);
                    break;
                default:
                    formattedValue = DefaultFormat(value, formatParameters);
                    break;
            }

            return formattedValue;

        }

        private static string FormatBoolean(bool p, FormatParametersList formatParameters)
        {
            if(p)
            {
                var truePresentation = formatParameters.GetParamValue(BOOLEAN_TRUE);
                if (truePresentation != null)
                    return truePresentation;
            }
            else
            {
                var falsePresentation = formatParameters.GetParamValue(BOOLEAN_FALSE);
                if (falsePresentation != null)
                    return falsePresentation;
            }

            return ValueFactory.Create(p).AsString();
        }

        private static string FormatNumber(decimal p, FormatParametersList formatParameters)
        {
            var locale = formatParameters.GetParamValue(LOCALE);
            NumberFormatInfo nf;
            if (locale != null)
            {
                var culture = System.Globalization.CultureInfo.CreateSpecificCulture(locale);
                nf = (NumberFormatInfo)culture.NumberFormat.Clone();

            }
            else
            {
                nf = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
            }

            string param;

            bool hasDigitLimits = false;
            int totalDigits = 0;
            int fractionDigits = 0;

            if(formatParameters.HasParam(NUM_MAX_SIZE, out param))
            {
                int paramToInt;
                if (Int32.TryParse(param, out paramToInt))
                {
                    if (paramToInt < 0)
                        paramToInt = 0;

                    hasDigitLimits = true;
                    totalDigits = paramToInt;
                }
            }

            if (formatParameters.HasParam(NUM_DECIMAL_SIZE, out param))
            {
                int paramToInt;
                if (Int32.TryParse(param, out paramToInt))
                {
                    if (paramToInt < 0)
                        paramToInt = 0;

                    hasDigitLimits = true;
                    fractionDigits = paramToInt;
                }
            }

            if(formatParameters.HasParam(NUM_FRACTION_DELIMITER, out param))
            {
                nf.NumberDecimalSeparator = param;
            }

            if (formatParameters.HasParam(NUM_GROUPS_DELIMITER, out param))
            {
                nf.NumberGroupSeparator = param;
            }
            else
            {
                nf.NumberGroupSeparator = " ";
            }

            if(formatParameters.HasParam(NUM_GROUPING, out param))
            {
                nf.NumberGroupSizes = ParseGroupSizes(param);
            }

            char leadingFormatSpecifier = '#';
            if(formatParameters.HasParam(NUM_LEADING_ZERO, out param))
            {
                leadingFormatSpecifier = '0';
            }

            StringBuilder formatBuilder = new StringBuilder();

            if (hasDigitLimits)
            {
                ApplyNumericSizeRestrictions(ref p, totalDigits, fractionDigits);

                formatBuilder.Append(leadingFormatSpecifier, totalDigits - fractionDigits);
                ApplyDigitsGrouping(formatBuilder, leadingFormatSpecifier, nf);
                
                formatBuilder.Append('.');
                formatBuilder.Append('#', fractionDigits);

            }
            else
            {
                int precision = GetDecimalPrecision(Decimal.GetBits(p));
                nf.NumberDecimalDigits = precision;
                formatBuilder.Append("N");
            }

            return p.ToString(formatBuilder.ToString(), nf);

        }

        private static int[] ParseGroupSizes(string param)
        {
            if(param == "" || param == "0")
                return new int[1]{0};

            List<int> sizes = new List<int>();
            for (int i = 0; i < param.Length; i++)
            {
                if (Char.IsNumber(param, i))
                {
                    sizes.Add(GetCharInteger(param[i]));
                }
            }

            return sizes.ToArray();
        }

        private static int GetCharInteger(char digit)
        {
            return (int)digit-0x30; // keycode offset
        }

        private static void ApplyNumericSizeRestrictions(ref decimal p, int totalDigits, int fractionDigits)
        {
            if (totalDigits == 0)
                return;

            decimal value = Math.Round(p, fractionDigits);
            int sign = Math.Sign(value);
            value = Math.Abs(value);

            if (totalDigits < fractionDigits)
                totalDigits = fractionDigits;
            
            if (totalDigits > decimal_digits_sizes[2])
                totalDigits = decimal_digits_sizes[2];

            var bits = Decimal.GetBits(value);

            int digits = 1;

            for (int i = 0; i < 3; i++)
            {
                if ((uint)bits[i] == uint.MaxValue)
                {
                    digits = decimal_digits_sizes[i];
                }
                else
                {
                    uint divided = (uint)bits[i];
                    if (divided == 0 && digits == 1)
                    {
                        digits = 0;
                        break;
                    }

                    while ((divided /= 10) >= 1)
                    {
                        digits++;
                    }
                    break;
                }
            }

            int power = GetDecimalPrecision(bits);
            int digitsLengthAvailable = totalDigits - power;
            
            if (digits-power > digitsLengthAvailable)
            {
                string fake = new String('9', totalDigits);
                int pointPos = totalDigits - fractionDigits;
                fake = fake.Insert(pointPos, ".");
                value = Decimal.Parse(fake, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo);
            }

            p = value * sign;
        }

        private static void ApplyDigitsGrouping(StringBuilder builder, char placeholder, NumberFormatInfo nf)
        {
            const char SEPARATOR_PLACEHOLDER = ',';

            int firstIndex = builder.Length;
            if (firstIndex <= 0)
                return;

            int offset = firstIndex;
            for (int i = 0; i < nf.NumberGroupSizes.Length; i++)
            {
                int size = nf.NumberGroupSizes[i];
                int insertionPoint = offset - size;
                if (insertionPoint <= 0)
                    break;

                builder.Insert(insertionPoint, SEPARATOR_PLACEHOLDER);
                offset -= size;
            }

            if(offset > 0)
            {
                int size = nf.NumberGroupSizes[nf.NumberGroupSizes.Length - 1];
                while(offset > 0)
                {
                    int insertionPoint = offset - size;
                    if (insertionPoint <= 0)
                        break;

                    builder.Insert(insertionPoint, SEPARATOR_PLACEHOLDER);
                    offset -= size;
                }
            }

        }

        private static int GetDecimalPrecision(int[] bits)
        {
            uint power = 0;
            unchecked
            {
                power = (uint)bits[3] & 0x00FF0000;
                power >>= 16;
            }

            return (int)power;
        }

        private static string FormatDate(DateTime dateTime, FormatParametersList formatParameters)
        {
            throw new NotImplementedException();
        }

        private static string DefaultFormat(IValue value, FormatParametersList formatParameters)
        {
            throw new NotImplementedException();
        }

        private static FormatParametersList ParseParameters(string format)
        {
            return new FormatParametersList(format);
        }

        
    }
}
