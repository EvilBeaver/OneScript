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
        
        private static FormatParameter[] ParseParameters(string format)
        {
            List<FormatParameter> paramList = new List<FormatParameter>();

            int index = 0;
            int len = format.Length;
            while(index < len)
            {
                SkipWhitespace(format, ref index);

                string param = ReadParameter(format, ref index);
                if (param == null)
                    break;
                
                SkipWhitespace(format, ref index);

                string value = ReadValue(format, ref index);
                if (value == null)
                    break;

                paramList.Add(new FormatParameter(param, value));
            }

            return paramList.ToArray();

        }

        private static void SkipWhitespace(string format, ref int index)
        {
            while(index < format.Length)
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
            while(index < format.Length)
            {
                if (Char.IsLetter(format, index))
                    index++;
                else if (format[index] == '=')
                {
                    var param = format.Substring(start, GetLength(start, index));
                    index++;
                    return param;
                }
                else if(Char.IsWhiteSpace(format, index))
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

            if(format[index] == SINGLE_QUOTE)
            {
                valueEnd = SINGLE_QUOTE;
                index++;
            }
            else if(format[index] == DOUBLE_QUOTE)
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

            while(index < format.Length)
            {
                if (format[index] == valueEnd)
                {
                    if (index+1 < format.Length && format[index + 1] == valueEnd)
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

        private string GetParamValue(FormatParameter[] parameters, string paramName)
        {
            throw new NotImplementedException();
        }

        public static string Format(IValue value, string format)
        {
            var formatParameters = ParseParameters(format);

            throw new NotImplementedException();
        }

        
    }
}
