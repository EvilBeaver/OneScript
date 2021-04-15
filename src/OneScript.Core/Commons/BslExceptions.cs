/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Localization;

namespace OneScript.Commons
{
    public static class BslExceptions
    {
        public static BslCoreException ConvertToNumberException()
        {
            return new TypeConversionException(new BilingualString(
                "Преобразование к типу 'Число' не поддерживается",
                "Conversion to type 'Number' is not supported"));
        }

        public static BslCoreException ConvertToBooleanException()
        {
            return new TypeConversionException(new BilingualString(
                "Преобразование к типу 'Булево' не поддерживается",
                "Conversion to type 'Boolean' is not supported"));
        }

        public static BslCoreException ConvertToDateException()
        {
            return new TypeConversionException(new BilingualString(
                "Преобразование к типу 'Дата' не поддерживается",
                "Conversion to type 'Date' is not supported"));
        }

        public static BslCoreException ValueIsNotObjectException()
        {
            return new TypeConversionException(new BilingualString(
                "Значение не является значением объектного типа",
                "Value is not of object type"));
        }
    }
}