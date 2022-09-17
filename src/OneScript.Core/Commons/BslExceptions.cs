/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Localization;
using OneScript.Values;

namespace OneScript.Commons
{
    public static class BslExceptions
    {
        private static string SourceTypeInfo(object source)
        {
            if (source == null) return "(null)";
            var typeDescription = source.GetType().ToString();
            var presentation = source.ToString();
            if (source is BslValue bslValue)
            {
                presentation = bslValue.ToString();
            }
            return  $"{typeDescription}: {presentation}";
        }
        
        public static RuntimeException ConvertToNumberException(object source = null)
        {
            var sourceTypeInfo = SourceTypeInfo(source);
            return new TypeConversionException(new BilingualString(
                $"{sourceTypeInfo} Преобразование к типу 'Число' не поддерживается",
                $"{sourceTypeInfo} Conversion to type 'Number' is not supported"));
        }

        public static RuntimeException ConvertToBooleanException()
        {
            return new TypeConversionException(new BilingualString(
                "Преобразование к типу 'Булево' не поддерживается",
                "Conversion to type 'Boolean' is not supported"));
        }

        public static RuntimeException ConvertToDateException()
        {
            return new TypeConversionException(new BilingualString(
                "Преобразование к типу 'Дата' не поддерживается",
                "Conversion to type 'Date' is not supported"));
        }

        public static RuntimeException ValueIsNotObjectException()
        {
            return new TypeConversionException(new BilingualString(
                "Значение не является значением объектного типа",
                "Value is not of object type"));
        }
    }
}