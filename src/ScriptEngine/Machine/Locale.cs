/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;

namespace ScriptEngine.Machine
{

    public static class Locale
    {
        public static string SystemLanguageISOName;

        public static string NStr(string src, string lang = null)
        {
            var parser = new FormatParametersList(src);
            string str;
            if (lang != null)
                str = parser.GetParamValue(lang);
            else
            {
                str = parser.GetParamValue(SystemLanguageISOName);
                if (str == null)
                    str = parser.GetParamValue("en");
                if (str == null)
                    str = parser.EnumerateValues().FirstOrDefault();
            }

            return str == null ? String.Empty : str;
        }
    }
}
