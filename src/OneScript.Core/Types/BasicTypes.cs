/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Values;

namespace OneScript.Types
{
    public static class BasicTypes
    {
        public static TypeDescriptor Number { get; }
        public static TypeDescriptor String { get; }
        public static TypeDescriptor Date { get; }
        public static TypeDescriptor Boolean { get; }
        public static TypeDescriptor Undefined { get; }
        public static TypeDescriptor Type { get; }
        public static TypeDescriptor Null { get; }
        
        /// <summary>
        /// Тип, устанавливаемый объектам, которые не задали свой тип
        /// Если значение этого типа возвращается функцией ТипЗнч - это ошибка автора объекта.
        /// </summary>
        public static TypeDescriptor UnknownType { get; }

        static BasicTypes()
        {
            Number = new TypeDescriptor(typeof(BslNumericValue), "Число", "Number");
            String = new TypeDescriptor(typeof(BslStringValue), "Строка", "String");
            Date = new TypeDescriptor(typeof(BslDateValue), "Дата", "Date");
            Boolean = new TypeDescriptor(typeof(BslBooleanValue), "Булево", "Boolean");
            Undefined = new TypeDescriptor(typeof(BslUndefinedValue), "Неопределено", "Undefined");
            Type = new TypeDescriptor(typeof(BslTypeValue), "Тип", "Type");
            Null = new TypeDescriptor(typeof(BslNullValue), "Null", "Null");
            UnknownType = new TypeDescriptor(typeof(BslValue), "$UnknownType$", "$UnknownType$");
        }
    }
}