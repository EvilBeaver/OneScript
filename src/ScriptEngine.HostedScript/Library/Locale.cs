﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptEngine.HostedScript.Library
{

    static class Locale
    {
        public static LanguagesEnum SystemLanguage;

        public static string NStr(string src, string lang = null)
        {
            var parser = new FormatParametersList(src);
            string str;
            if (lang != null)
                str = parser.GetParamValue(lang);
            else
            {
                if (SystemLanguage == LanguagesEnum.Russian)
                {
                    str = parser.GetParamValue("ru");
                    if (str == null)
                        str = parser.GetParamValue("en");
                }
                else
                {
                    str = parser.GetParamValue("en");
                    if (str == null)
                        str = parser.GetParamValue("ru");
                }
                if (str == null)
                    str = parser.EnumerateValues().FirstOrDefault();
            }

            return str == null ? String.Empty : str;
        }
    }
}
