/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;

namespace OneScript.Localization
{
    public class BilingualString : IEquatable<BilingualString>
    {
        private static readonly CultureInfo RussianCulture;

        static BilingualString()
        {
            try
            {
                RussianCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("ru");
            }
            catch (CultureNotFoundException)
            {
            }
        }

        public static implicit operator BilingualString(string source)
        {
            return new BilingualString(source);
        }
        
        public static implicit operator string(BilingualString str)
        {
            return str.ToString();
        }
        
        public BilingualString(string ru, string en)
        {
            Russian = ru;
            English = en;
        }
        
        public BilingualString(string single)
        {
            Russian = single;
            English = string.Empty;
        }

        public string Russian { get; }
        
        public string English { get; }

        public override string ToString()
        {
            return Localize(Russian, English);
        }

        public static bool UseRussianLocale => CultureInfo.CurrentCulture.Parent.Equals(RussianCulture);

        public static string Localize(string russian, string english)
        {
            var currentCulture = CultureInfo.CurrentCulture;
            if (!Equals(currentCulture.Parent, RussianCulture))
            {
                return string.IsNullOrEmpty(english) ? russian : english;
            }

            return russian;
        }

        public bool Equals(BilingualString other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Russian == other.Russian && English == other.English;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BilingualString)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Russian, English);
        }
    }
}