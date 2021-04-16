/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Language;

namespace ScriptEngine.Machine
{
    public class RuntimeException : ScriptException
    {
        
        public RuntimeException(string msg) : base(msg)
        {
        }

        public RuntimeException(string msg, Exception inner)
            : base(new ErrorPositionInfo(), msg, inner)
        {
        }

        public static BslRuntimeException DeprecatedMethodCall(string name)
        {
            return new BslRuntimeException($"Вызов безнадёжно устаревшего метода {name}");
        }

        public static BslRuntimeException ConvertToNumberException()
        {
            return new BslRuntimeException("Преобразование к типу 'Число' не поддерживается");
        }

        public static BslRuntimeException ConvertToBooleanException()
        {
            return new BslRuntimeException("Преобразование к типу 'Булево' не поддерживается");
        }

        public static BslRuntimeException ConvertToDateException()
        {
            return new BslRuntimeException("Преобразование к типу 'Дата' не поддерживается");
        }

        public static BslRuntimeException PropIsNotReadableException(string prop)
        {
            return PropertyAccessException.GetPropIsNotReadableException(prop);
        }

        public static BslRuntimeException PropIsNotWritableException(string prop)
        {
            return PropertyAccessException.GetPropIsNotWritableException(prop);
        }

        public static BslRuntimeException PropNotFoundException(string prop)
        {
            return PropertyAccessException.GetPropNotFoundException(prop);
        }
        
        public static BslRuntimeException MethodNotFoundException(string methodName)
        {
            return new BslRuntimeException($"Метод объекта не обнаружен ({methodName})");
        }

        public static BslRuntimeException MethodNotFoundException(string methodName, string objectName)
        {
            return new BslRuntimeException($"Метод объекта не обнаружен ({{{objectName}}}::{methodName})");
        }

        public static BslRuntimeException ValueIsNotObjectException()
        {
            return new BslRuntimeException("Значение не является значением объектного типа");
        }

        public static BslRuntimeException TooManyArgumentsPassed()
        {
            return new BslRuntimeException("Слишком много фактических параметров");
        }

        public static BslRuntimeException TooFewArgumentsPassed()
        {
            return new BslRuntimeException("Недостаточно фактических параметров");
        }

        public static BslRuntimeException InvalidArgumentType()
        {
            return new BslRuntimeException("Неверный тип аргумента");
        }

        public static BslRuntimeException InvalidArgumentType(string argName)
        {
            return new BslRuntimeException(String.Format("Неверный тип аргумента '{0}'", argName));
        }

        public static BslRuntimeException InvalidNthArgumentType(int argNum)
        {
            return new BslRuntimeException(String.Format("Неверный тип аргумента номер {0}", argNum));
        }

        public static BslRuntimeException InvalidArgumentType(int argNum, string argName )
        {
            return new BslRuntimeException(String.Format("Неверный тип аргумента номер {0} '{1}'", argNum, argName ));
        }

        public static BslRuntimeException InvalidArgumentValue()
        {
            return new BslRuntimeException("Неверное значение аргумента");
        }

        public static BslRuntimeException InvalidNthArgumentValue(int argNum)
        {
            return new BslRuntimeException(String.Format("Неверное значение аргумента номер {0}", argNum));
        }

        public static BslRuntimeException InvalidArgumentValue(object value)
        {
            return new BslRuntimeException("Неверное значение аргумента {"+value.ToString()+"}");
        }

        public static BslRuntimeException ComparisonNotSupportedException()
        {
            return new BslRuntimeException("Сравнение на больше/меньше для данного типа не поддерживается");
        }

        public static BslRuntimeException IndexedAccessIsNotSupportedException()
        {
            return new BslRuntimeException("Объект не поддерживает доступ по индексу");
        }

        public static BslRuntimeException IteratorIsNotDefined()
        {
            return new BslRuntimeException("Итератор не определен");
        }

        public static BslRuntimeException UseProcAsAFunction()
        {
            return new BslRuntimeException("Использование процедуры, как функции");
        }

        public static BslRuntimeException DivideByZero()
        {
            return new BslRuntimeException("Деление на ноль");
        }

        public static BslRuntimeException ConstructorNotFound(string typeName)
        {
            var template = Locale.NStr("ru = 'Конструктор не найден ({0})';" +
                                       "en = 'Constructor not found ({0})'");

            return new BslRuntimeException(string.Format(template, typeName));
        }
    }

    public class WrongStackConditionException : ApplicationException
    {
        public WrongStackConditionException()
            : base("Внутренняя ошибка: неверное состояние стека")
        {

        }
    }

    public class PropertyAccessException : BslRuntimeException
    {
        private PropertyAccessException(string msg) : base (msg)
        {

        }

        public static PropertyAccessException GetPropIsNotReadableException(string prop)
        {
            return new PropertyAccessException(string.Format("Свойство {0} недоступно для чтения", prop));
        }

        public static PropertyAccessException GetPropIsNotWritableException(string prop)
        {
            return new PropertyAccessException(string.Format("Свойство {0} недоступно для записи", prop));
        }

        public static PropertyAccessException GetPropNotFoundException(string prop)
        {
            return new PropertyAccessException(string.Format("Свойство объекта не обнаружено ({0})", prop));
        }

    }

    public class ScriptInterruptionException : ApplicationException
    {
        public ScriptInterruptionException(int exitCode) : base("Script interrupted")
        {
            ExitCode = exitCode;
        }

        public int ExitCode { get; private set; }
    }

    public class ValueMarshallingException : BslRuntimeException
    {
        public ValueMarshallingException() : this("Неклассифицированная ошибка маршаллинга значений")
        {
        }

        public ValueMarshallingException(string message) : base(message)
        {
        }
    }
}
