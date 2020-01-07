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

namespace ScriptEngine
{
    public class FormatParametersList
    {
        private struct FormatParameter
        {
            private readonly string _name;
            private readonly string _value;

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

        readonly List<FormatParameter> _paramList = new List<FormatParameter>();

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
                if (Char.IsLetterOrDigit(format, index) || format[index] == '.' || format[index] == '_')
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
            return FindParamValue(x => String.Compare(x.Name, paramName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public string GetParamValue(string paramNameRus, string paramNameEng)
        {
            return FindParamValue(x => String.Compare(x.Name, paramNameRus, StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(x.Name, paramNameEng, StringComparison.OrdinalIgnoreCase) == 0);

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

        public Dictionary<string, string> ToDictionary()
        {
            var result = new Dictionary<string, string>();
            foreach (var item in _paramList)
            {
                result[item.Name] = item.Value;
            }

            return result;
        }

        public IEnumerable<string> EnumerateValues()
        {
            return _paramList.Select(x => x.Value);
        }

        private string FindParamValue(Predicate<FormatParameter> criteria)
        {
            var param = _paramList.FindLast(criteria);
            if (param.Name == null)
                return null;

            return param.Value;
        }

    }
}
