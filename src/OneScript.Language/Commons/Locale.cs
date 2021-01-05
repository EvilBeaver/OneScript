/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace OneScript.Commons
{

    public static class Locale
    {
        private static string _actualLocaleName;

        public static string SystemLanguageISOName
        {
            get => _actualLocaleName;
            set
            {
                _actualLocaleName = value;
                SystemLocaleChanged?.Invoke();
            }
        }

        public static event Action SystemLocaleChanged;

        /// <summary>
        ///
        /// По умолчанию, системный язык определяется настройкой локали в операционной системе. 
        /// Также, системный языкможет быть задан в файле oscript.cfg параметром systemlanguage.
        /// Сама функция НСтр работает следующим образом:
        ///     Если подстрока для системного языка присутствует, то она и возвращается, 
        ///     иначе, возвращается подстрока для английского языка (en), 
        ///     если подстрока для английского языка не задана, то возвращается первая по порядку подстрока.
        ///     Если подстрока для системного языка присутствует, то она и возвращается, 
        ///     иначе, возвращается подстрока для английского языка(en), 
        ///     если подстрока для английского языка не задана, то возвращается первая по порядку подстрока.
        /// </summary>
        ///     
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
