/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Commons;

namespace ScriptEngine.Machine
{
    public class WrongStackConditionException : ApplicationException
    {
        public WrongStackConditionException()
            : base("Внутренняя ошибка: неверное состояние стека")
        {

        }
    }
    
    public class ValueMarshallingException : RuntimeException
    {
        public ValueMarshallingException() : this("Неклассифицированная ошибка маршаллинга значений")
        {
        }

        public ValueMarshallingException(string message) : base(message)
        {
        }

        public static ValueMarshallingException TypeNotSupported(Type type)
        {
            return new ValueMarshallingException(string.Format(Locale.NStr
              ("ru='Возвращаемый тип {0} не поддерживается'; en='Return type {0} is not supported'"), type));
        }

        public static ValueMarshallingException InvalidEnum(Type type)
        {
            return new ValueMarshallingException(string.Format(Locale.NStr
              ("ru = 'Некорректный тип конвертируемого перечисления: {0}'; en = 'Invalid enum return type: {0}'"), type));
        }

        public static ValueMarshallingException EnumWithNoAttribute(Type type)
        {
            return new ValueMarshallingException(string.Format(Locale.NStr
              ("ru='Значение перечисления {0} должно быть помечено атрибутом EnumItemAttribute';"
              + "en='An enumeration value {0} must be marked with the EnumItemAttribute attribute"), type));
        }

        public static ValueMarshallingException NoConversionToCLR(Type type)
        {
            return new ValueMarshallingException(string.Format(Locale.NStr
              ("ru='Тип {0} не поддерживает преобразование в CLR-объект';"
              +"en='Type {0} does not support conversion to CLR object'"), type));
        }
        public static ValueMarshallingException InvalidNullValue()
        {
            return new ValueMarshallingException(Locale.NStr
              ("ru = 'Значение не может быть null'; en = 'Value cannot be null'"));
        }

        public static ValueMarshallingException InvalidNullIndex()
        {
            return new ValueMarshallingException(Locale.NStr
                ("ru = 'Индекс не может быть null'; en = 'Index cannot be null'"));
        }

    }
}
