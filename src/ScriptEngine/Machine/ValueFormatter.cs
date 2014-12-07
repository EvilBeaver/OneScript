using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    static class ValueFormatter
    {
        static readonly string[] BOOLEAN_FALSE = { "БЛ", "BT" };
        static readonly string[] BOOLEAN_TRUE = { "БИ", "BF" };
        static readonly string[] LOCALE = { "Л", "L" };
        static readonly string[] NUM_MAX_SIZE = { "ЧЦ", "ND" };
        static readonly string[] NUM_DECIMAL_SIZE = { "ЧДЦ", "NFD" };

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
                        break;

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
                bool success = false;

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
                        success = true;
                        break;
                    }
                    else if (valueEnd == SPACE && format[index] == ';')
                    {
                        success = true;
                        break;
                    }
                    else
                        valueBuffer.Append(format[index++]);
                }

                if (success)
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

                    return valueBuffer.ToString();
                }
                else
                    return null;

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
                if(paramValue != null)
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

            //var locale = formatParameters.GetParamValue(LOCALE);
            //if (locale != null)
                //throw new NotImplementedException("Explicit localization of booleans isn't implemented yet");

            return ValueFactory.Create(p).AsString();
        }

        private static string FormatNumber(decimal p, FormatParametersList formatParameters)
        {
            var locale = formatParameters.GetParamValue(LOCALE);
            System.Globalization.NumberFormatInfo nf;
            if (locale != null)
            {
                var culture = System.Globalization.CultureInfo.CreateSpecificCulture(locale);
                nf = culture.NumberFormat;

            }
            else
            {
                nf = System.Globalization.NumberFormatInfo.CurrentInfo;
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
                    hasDigitLimits = true;
                    totalDigits = paramToInt;
                }
            }

            if (formatParameters.HasParam(NUM_DECIMAL_SIZE, out param))
            {
                int paramToInt;
                if (Int32.TryParse(param, out paramToInt))
                {
                    hasDigitLimits = true;
                    fractionDigits = paramToInt;
                }
            }

            if(hasDigitLimits)
            {
                ApplyNumericSizeRestrictions(ref p, totalDigits, fractionDigits);
            }

            return p.ToString(nf);

        }

        private static void ApplyNumericSizeRestrictions(ref decimal p, int totalDigits, int fractionDigits)
        {
            decimal value = Math.Round(p, fractionDigits);
            int sign = Math.Sign(value);
            value = Math.Abs(value);

            if (totalDigits < fractionDigits)
                totalDigits = fractionDigits;

            int integerPartLen = totalDigits - fractionDigits;
            long realIntegerPart = (long)value;
            long realFractions = (long)((value - realIntegerPart) * (10 ^ fractionDigits));
            // some magic should be done here )

            p = value;

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
