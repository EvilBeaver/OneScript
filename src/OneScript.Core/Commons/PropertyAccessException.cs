/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Localization;

namespace OneScript.Commons
{
    public class PropertyAccessException : RuntimeException
    {
        public PropertyAccessException(BilingualString message, Exception innerException) : base(message, innerException)
        {
        }

        public PropertyAccessException(BilingualString message) : base(message)
        {
        }
        
        public static PropertyAccessException PropIsNotReadableException(string prop)
        {
            return new PropertyAccessException(new BilingualString(
                $"Свойство {prop} недоступно для чтения",
                $"Property {prop} is not readable"));
        }

        public static PropertyAccessException PropIsNotWritableException(string prop)
        {
            return new PropertyAccessException(new BilingualString(
                $"Свойство {prop} недоступно для записи",
                $"Property {prop} is not writable"));
        }

        public static PropertyAccessException PropNotFoundException(string prop)
        {
            return new PropertyAccessException(new BilingualString(
                $"Свойство объекта не обнаружено ({prop})",
                $"Object property is not found ({prop})"));
        }
    }
}