/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Core;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Values;

namespace ScriptEngine.Types
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
            Number = new TypeDescriptor(
                new Guid("A7403601-38B7-420A-8630-84A1F675E102"),
                "Число",
                "Number",
                typeof(NumberValue));
            
            String = new TypeDescriptor(
                new Guid("1AF7A333-33A1-45E2-BC5E-65CE2455D619"), 
                "Строка",
                "String",
                typeof(StringValue));
            
            Date = new TypeDescriptor(new Guid("A7E20D06-E876-4949-9DB2-7C93891F3B87"), 
                "Дата",
                "Date",
                typeof(DateValue));
            
            Boolean = new TypeDescriptor(new Guid("34EDD2E3-DD29-4829-A424-67B3404AF423"), 
                "Булево",
                "Boolean",
                typeof(BooleanValue));
            
            Undefined = new TypeDescriptor(new Guid("783CE532-8CE0-4C59-BEF4-835AEFB715E4"),
                "Неопределено",
                "Undefined",
                typeof(UndefinedValue));
            
            Type = new TypeDescriptor(new Guid("4BCF3B78-B525-4159-8180-C76EDA613652"),
                "Тип",
                "Type",
                typeof(TypeTypeValue));
            
            Null = new TypeDescriptor(new Guid("26D78088-915A-4294-97E1-FB39E70187A6"),
                "Null",
                "Null",
                typeof(NullValue));

            UnknownType = new TypeDescriptor(new Guid("EC3408A3-18A7-4A38-936F-7E7094D3C4E3"),
                "$UnknownType$",
                "$UnknownType$",
                typeof(IValue));
        }
    }
}