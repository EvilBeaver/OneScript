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
    [Obsolete]
    public static class OldRuntimeException
    {
        public static RuntimeException PropNotFoundException(string prop)
        {
            return PropertyAccessException.PropNotFoundException(prop);
        }
    }

    public class WrongStackConditionException : ApplicationException
    {
        public WrongStackConditionException()
            : base("Внутренняя ошибка: неверное состояние стека")
        {

        }
    }

    public class PropertyAccessException : RuntimeException
    {
        private PropertyAccessException(string msg) : base (msg)
        {

        }

        public static PropertyAccessException PropIsNotReadableException(string prop)
        {
            return new PropertyAccessException(string.Format("Свойство {0} недоступно для чтения", prop));
        }

        public static PropertyAccessException PropIsNotWritableException(string prop)
        {
            return new PropertyAccessException(string.Format("Свойство {0} недоступно для записи", prop));
        }

        public static PropertyAccessException PropNotFoundException(string prop)
        {
            return new PropertyAccessException(string.Format("Свойство объекта не обнаружено ({0})", prop));
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
    }
}
