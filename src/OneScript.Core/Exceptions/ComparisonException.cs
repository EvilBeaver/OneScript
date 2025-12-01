/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Localization;

namespace OneScript.Exceptions
{
    public class ComparisonException : RuntimeException
    {
        public ComparisonException(BilingualString message) : base(message)
        {
        }

        public static ComparisonException NotSupported()
        {
            return new ComparisonException(new BilingualString(
                $"Сравнение на больше/меньше для данного типа не поддерживается",
                $"Greater-than/Less-than comparison operations are not supported"));
        }
        
        public static ComparisonException NotSupported(string type)
        {
            return new ComparisonException(new BilingualString(
                $"Сравнение на больше/меньше для типа '{type}' не поддерживается",
                $"Greater-than/Less-than comparison operations are not supported for '{type}'"));
        }
        
        public static ComparisonException NotSupported(string type1, string type2)
        {
            return new ComparisonException(new BilingualString(
                $"Сравнение на больше/меньше типов '{type1}' и '{type2}' не поддерживается",
                $"Greater-than/Less-than comparison operations are not supported for '{type1}' and '{type2}'"));
        }
    }
}