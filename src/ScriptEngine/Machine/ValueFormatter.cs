/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using OneScript.Commons;
using OneScript.Values;

namespace ScriptEngine.Machine
{
    public static class ValueFormatter
    {
        static readonly string[] BOOLEAN_FALSE = { "БЛ", "BF" };
        static readonly string[] BOOLEAN_TRUE = { "БИ", "BT" };
        static readonly string[] LOCALE = { "Л", "L" };
        static readonly string[] NUM_MAX_SIZE = { "ЧЦ", "ND" };
        static readonly string[] NUM_DECIMAL_SIZE = { "ЧДЦ", "NFD" };
        static readonly string[] NUM_FRACTION_DELIMITER = { "ЧРД", "NDS" };
        static readonly string[] NUM_GROUPS_DELIMITER = { "ЧРГ", "NGS" };
        static readonly string[] NUM_ZERO_APPEARANCE = { "ЧН", "NZ" };
        static readonly string[] NUM_GROUPING = { "ЧГ", "NG" };
        static readonly string[] NUM_LEADING_ZERO = { "ЧВН", "NLZ" };
        static readonly string[] NUM_NEGATIVE_APPEARANCE = { "ЧО", "NN" };
        static readonly string[] DATE_EMPTY = { "ДП", "DE" };
        static readonly string[] DATE_FORMAT = { "ДФ", "DF" };
        static readonly string[] DATE_LOCAL_FORMAT = { "ДЛФ", "DLF" };

        const int MAX_DECIMAL_ROUND = 28; 

        public static string Format(IValue value, string format)
        {
            var formatParameters = ParseParameters(format);

            string formattedValue;

            switch(value.GetRawValue())
            {
                case BslBooleanValue _:
                    formattedValue = FormatBoolean(value.AsBoolean(), formatParameters);
                    break;
                case BslNumericValue _:
                    formattedValue = FormatNumber(value.AsNumber(), formatParameters);
                    break;
                case BslDateValue _:
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

        #region Number formatting

        private static int ParseUnsignedParam(string param)
        {
            int paramToInt;
            return (Int32.TryParse(param, out paramToInt) && paramToInt > 0) ? paramToInt : 0;
        }

        private static string FormatNumber(decimal num, FormatParametersList formatParameters)
        {
            int[] numberGroupSizes = null;

            var locale = formatParameters.GetParamValue(LOCALE);
            NumberFormatInfo nf;
            if (locale != null)
            {
                // culture codes in 1C-style
                var culture = CreateCulture(locale);
                nf = (NumberFormatInfo)culture.NumberFormat.Clone();
            }
            else
            {
                nf = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
            }

            string param;
            string zeroAppearance;

            if (formatParameters.HasParam(NUM_ZERO_APPEARANCE, out param))
            {
                zeroAppearance = (param == "") ? "0" : param;
            }
            else
                zeroAppearance = "";

            if (num == 0)
            {
                return zeroAppearance;
            }

            bool hasDigitLimits = false;
            int totalDigits = 0;
            int fractionDigits = 0;
            bool largeGroupSize = false;

            if (formatParameters.HasParam(NUM_MAX_SIZE, out param))
            {
                hasDigitLimits = true;
                totalDigits = ParseUnsignedParam(param);
            }

            if (formatParameters.HasParam(NUM_DECIMAL_SIZE, out param))
            {
                hasDigitLimits = true;
                fractionDigits = ParseUnsignedParam(param);
            }

            if (formatParameters.HasParam(NUM_FRACTION_DELIMITER, out param))
            {
                if (param.Length > 0)
                    nf.NumberDecimalSeparator = (param.Length < 2 ? param : param.Substring(0, 1));
            }

            if (formatParameters.HasParam(NUM_GROUPS_DELIMITER, out param))
            {
                if (param.Length>0)
                    nf.NumberGroupSeparator = (param.Length < 2 ? param : param.Substring(0, 1));
            }

            if (formatParameters.HasParam(NUM_GROUPING, out param))
            {
                numberGroupSizes = ParseGroupSizes(param);
                if (numberGroupSizes.Any(x => x > 9))
                {
                    nf.NumberGroupSizes = new int[] { 0 };
                    largeGroupSize = true;
                }
                else
                {
                    nf.NumberGroupSizes = numberGroupSizes;
                }
            }

            if (formatParameters.HasParam(NUM_NEGATIVE_APPEARANCE, out param))
            {
                int pattern;
                if (int.TryParse(param, out pattern))
                    nf.NumberNegativePattern = (pattern >= 0 && pattern <= 4 ? pattern : 1);
            }

            bool hasLeadingZeroes = formatParameters.HasParam(NUM_LEADING_ZERO, out param);
 
            StringBuilder formatBuilder = new StringBuilder();

            if (hasDigitLimits)
            {
                bool overflov = !ApplyNumericSizeRestrictions(ref num, totalDigits, fractionDigits); ;

                if (num == 0)
                    return zeroAppearance;

                if (totalDigits == 0)
                {
                    formatBuilder.Append("#,0.");
                    formatBuilder.Append('0', fractionDigits);
                }
                else
                {
                    int intDigits = totalDigits - fractionDigits;

                    if (intDigits > 1)
                    {
                        if( hasLeadingZeroes )
                            formatBuilder.Append('0', intDigits - 1);
                        else
                            formatBuilder.Append('#', 1);
                        formatBuilder.Append(',');
                    }

                    if (intDigits > 0)
                    {
                        formatBuilder.Append("0.");
                        if (overflov && totalDigits > MAX_DECIMAL_ROUND)
                        {
                            if (intDigits < MAX_DECIMAL_ROUND)
                                formatBuilder.Append('0', MAX_DECIMAL_ROUND - intDigits);
                            formatBuilder.Append('9', totalDigits - MAX_DECIMAL_ROUND);
                        }
                        else
                        {
                            formatBuilder.Append('0', fractionDigits);
                        }
                    }
                    else
                    {
                        largeGroupSize = false;
                        formatBuilder.Append("#.");
                        if (overflov && totalDigits > MAX_DECIMAL_ROUND)
                        {
                            formatBuilder.Append('0', MAX_DECIMAL_ROUND);
                            formatBuilder.Append('9', totalDigits - MAX_DECIMAL_ROUND);
                        }
                        else
                        {
                            formatBuilder.Append('0', totalDigits);
                        }
                    }
                }

                if (num < 0)
                    ApplyNegativePattern(formatBuilder, nf);
            }
            else
            {
                int precision = GetDecimalPrecision(Decimal.GetBits(num));
                nf.NumberDecimalDigits = precision;
                formatBuilder.Append('N');
            }

            if (largeGroupSize)
            {
                string decSeparator = nf.NumberDecimalSeparator;
                nf.NumberDecimalSeparator = ".";
                string preformatted = num.ToString(formatBuilder.ToString(), nf);
                nf.NumberDecimalSeparator = decSeparator;

                return ApplyDigitsGrouping( preformatted, nf, numberGroupSizes );
            }

            return num.ToString(formatBuilder.ToString(), nf);
        }

        private static int[] ParseGroupSizes(string param)
        {
            List<int> sizes = new List<int>();
            for (int i = 0, ngroup=0; ngroup<2; ++ngroup )
            {
                while (i < param.Length && !Char.IsNumber(param, i)) ++i;
                int start = i;
                while (i < param.Length && Char.IsNumber(param, i)) ++i;

                int value = 0;
                if (i > start)
                {
                    value = Int32.Parse(param.Substring(start, i-start));
                    if (value == 0 && ngroup > 0)
                        break;
                }

                sizes.Add(value);
                if( value==0 )
                   break;
            }

            return sizes.ToArray();
        }

        public static void ApplyNegativePattern(StringBuilder formatBuilder, NumberFormatInfo nf)
        {
            switch (nf.NumberNegativePattern)
            {
                case 0:
                    formatBuilder.Insert(0, '(');
                    formatBuilder.Append(')');
                    break;
                case 1:
                    formatBuilder.Insert(0, nf.NegativeSign);
                    break;
                case 2:
                    formatBuilder.Insert(0, ' ');
                    formatBuilder.Insert(0, nf.NegativeSign);
                    break;
                case 3:
                    formatBuilder.Append(nf.NegativeSign);
                    break;
                case 4:
                    formatBuilder.Append(' ');
                    formatBuilder.Append(nf.NegativeSign);
                    break;
            }
            formatBuilder.Insert(0, ';');
        }

        public static bool ApplyNumericSizeRestrictions(ref decimal num, int totalDigits, int fractionDigits)
        {
            if (fractionDigits <= MAX_DECIMAL_ROUND)
                num = Math.Round(num, fractionDigits, MidpointRounding.AwayFromZero);

            if (totalDigits == 0)
                return true;

            decimal intVal = Math.Truncate(num);
            if (intVal == 0)
            {
                if (totalDigits < fractionDigits && totalDigits <= MAX_DECIMAL_ROUND)
                    num = Math.Round(num, totalDigits, MidpointRounding.AwayFromZero);
                return true;
            }

            int digits = Math.Abs(intVal).ToString(NumberFormatInfo.InvariantInfo).Length; // weird but fast

            if (digits > totalDigits - fractionDigits)
            {
                string fake = new String('9', totalDigits <= MAX_DECIMAL_ROUND ? totalDigits : MAX_DECIMAL_ROUND);
                int pointPos = totalDigits - fractionDigits;
                if (pointPos < 0) pointPos = 0;
                fake = fake.Insert(pointPos, ".");
                num = Decimal.Parse(fake, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo);
                if (intVal < 0) num = -num;
                return false;
            }
            return true;
        }

        private static string ApplyDigitsGrouping(string str, NumberFormatInfo nf, int[] numberGroupSizes )
        {
            StringBuilder builder = new StringBuilder(str);
            int decPos = str.IndexOf('.');

            int firstIndex = decPos>=0 ? decPos: builder.Length;

            int size = numberGroupSizes[0];
            if (size == 0)
                return str.Replace(".", nf.NumberDecimalSeparator);

            int offset = firstIndex;
            int insertionPoint = offset - size;
            if (insertionPoint <= 0)
               return str.Replace(".", nf.NumberDecimalSeparator);

            builder.Insert(insertionPoint, nf.NumberGroupSeparator);
            offset -= size;

            if (numberGroupSizes.Length > 1)
            {
              size = numberGroupSizes[1];
            }

            if ( size > 0 )
            {
                while (offset > 0)
                {
                    insertionPoint = offset - size;
                    if (insertionPoint <= 0)
                       break;

                    builder.Insert(insertionPoint, nf.NumberGroupSeparator);
                    offset -= size;
                }
            }

            if (decPos >= 0)
            {
                builder.Replace(".", nf.NumberDecimalSeparator, decPos, str.Length - decPos);
            }
            
            return builder.ToString();
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

#endregion

#region Date formatting

        private static string FormatDate(DateTime dateTime, FormatParametersList formatParameters)
        {
            var locale = formatParameters.GetParamValue(LOCALE);
            DateTimeFormatInfo df;
            if (locale != null)
            {
                // culture codes in 1C-style
                var culture = CreateCulture(locale);
                df = (DateTimeFormatInfo)culture.DateTimeFormat.Clone();
            }
            else
            {
                var currentDF= DateTimeFormatInfo.CurrentInfo;
                if(currentDF == null)
                    df = new DateTimeFormatInfo();
                else
                    df = (DateTimeFormatInfo)currentDF.Clone();
            }

            string param;

            if (dateTime == DateTime.MinValue)
            {
                if (formatParameters.HasParam(DATE_EMPTY, out param))
                {
                    return param;
                }

                return String.Empty;
            }

            string formatString = "G";

            if (formatParameters.HasParam(DATE_FORMAT, out param))
            {
                formatString = ProcessDateFormat(param);
            }

            if (formatParameters.HasParam(DATE_LOCAL_FORMAT, out param))
            {
                formatString = ProcessLocalDateFormat(param);
            }

            return dateTime.ToString(formatString, df);

        }

        private static string ProcessDateFormat(string param)
        {
            var builder = new StringBuilder(param);
            for (int i = 0; i < param.Length; i++)
            {
                if (param[i] == 'д')
                    builder[i] = 'd';

                if (param[i] == 'М')
                    builder[i] = 'M';
                
                if (param[i] == 'г')
                    builder[i] = 'y';

                if (param[i] == 'к')
                    builder[i] = 'q';

                if (param[i] == 'ч')
                    builder[i] = 'h';
                if (param[i] == 'Ч')
                    builder[i] = 'H';
                if (param[i] == 'м')
                    builder[i] = 'm';
                if (param[i] == 'с')
                    builder[i] = 's';

                if (param[i] == 'в' && i + 1 < param.Length && param[i + 1] == 'в')
                {
                    builder[i] = 't';
                    builder[i + 1] = 't';
                    i++;
                }

                if (param[i] == 'р')
                {
                    builder[i] = 'f';
                }
                
            }
            builder.Replace("/", "\\/");
            builder.Replace("%", "\\%");

            return builder.ToString();
        }

        private static string ProcessLocalDateFormat(string param)
        {
            const string DATETIME_RU = "ДВ";
            const string DATETIME_EN = "DT";
            const string DATE_RU = "Д";
            const string DATE_EN = "D";
            const string LONG_DATE_RU = "ДД";
            const string LONG_DATE_EN = "DD";
            const string LONG_DATETIME_RU = "ДДВ";
            const string LONG_DATETIME_EN = "DDT";
            const string TIME_RU = "В";
            const string TIME_EN = "T";

            param = param.ToUpper();
            switch (param)
            {
                case DATETIME_RU:
                case DATETIME_EN:
                    return "G";
                case LONG_DATETIME_EN:
                case LONG_DATETIME_RU:
                    return "F";
                case LONG_DATE_EN:
                case LONG_DATE_RU:
                    return "D";
                case DATE_EN:
                case DATE_RU:
                    return "d";
                case TIME_EN:
                case TIME_RU:
                    return "T";
                default:
                    return "G";
            }
        } 

#endregion

        private static string DefaultFormat(IValue value, FormatParametersList formatParameters)
        {
            return value.AsString();
        }

        private static FormatParametersList ParseParameters(string format)
        {
            return new FormatParametersList(format);
        }

        private static CultureInfo CreateCulture(string locale)
        {
            // преобразуем имя локали из нашего формата в формат понятный для .NET
            var isoLocaleName = locale.Replace('_', '-');
            return Locale.CreateCulture(isoLocaleName);
        }


        
    }
}
