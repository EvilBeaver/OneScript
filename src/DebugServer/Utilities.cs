using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DebugServer
{
    static class Utilities
    {
        private static readonly Regex VARIABLE = new Regex(@"\{(\w+)\}");

        public static string ConcatArguments(IEnumerable args)
        {
            var sb = new StringBuilder();
            foreach (var stringArg in args)
            {
                sb.Append(' ');
                sb.Append('\"');
                sb.Append(stringArg.ToString());
                sb.Append('\"');
            }

            return sb.ToString();
        }

        public static string ExpandVariables(string format, dynamic variables, bool underscoredOnly = true)
        {
            if (variables == null)
            {
                variables = new { };
            }
            Type type = variables.GetType();
            return VARIABLE.Replace(format, match => {
                string name = match.Groups[1].Value;
                if (!underscoredOnly || name.StartsWith("_"))
                {

                    PropertyInfo property = type.GetProperty(name);
                    if (property != null)
                    {
                        object value = property.GetValue(variables, null);
                        return value.ToString();
                    }
                    return '{' + name + ": not found}";
                }
                return match.Groups[0].Value;
            });
        }
    }
}
