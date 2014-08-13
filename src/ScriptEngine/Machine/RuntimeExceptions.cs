using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public class RuntimeException : ScriptException
    {
        public RuntimeException() : base()
        {
        }

        public RuntimeException(string msg) : base(msg)
        {
        }

        public RuntimeException(string msg, Exception inner)
            : base(new CodePositionInfo(), msg, inner)
        {
        }

        public static RuntimeException ConvertToNumberException()
        {
            return new RuntimeException("Преобразование к типу 'Число' не поддерживается");
        }

        public static RuntimeException ConvertToBooleanException()
        {
            return new RuntimeException("Преобразование к типу 'Булево' не поддерживается");
        }

        public static RuntimeException ConvertToDateException()
        {
            return new RuntimeException("Преобразование к типу 'Дата' не поддерживается");
        }

        public static RuntimeException PropIsNotReadableException(string prop)
        {
            return PropertyAccessException.GetPropIsNotReadableException(prop);
        }

        public static RuntimeException PropIsNotWritableException(string prop)
        {
            return PropertyAccessException.GetPropIsNotWritableException(prop);
        }

        public static RuntimeException PropNotFoundException(string prop)
        {
            return PropertyAccessException.GetPropNotFoundException(prop);
        }
        
        public static RuntimeException MethodNotFoundException(string methodName)
        {
            return new RuntimeException(string.Format("Метод объекта не обнаружен ({0})", methodName));
        }

        public static RuntimeException ValueIsNotObjectException()
        {
            return new RuntimeException("Значение не является значением объектного типа");
        }

        public static RuntimeException TooManyArgumentsPassed()
        {
            return new RuntimeException("Слишком много фактических параметров");
        }

        public static RuntimeException TooLittleArgumentsPassed()
        {
            return new RuntimeException("Недостаточно фактических параметров");
        }

        public static RuntimeException ArgHasNoDefaultValue(int argNum)
        {
            return new RuntimeException(string.Format("Аргумент {0} не имеет значения по умолчанию", argNum));
        }

        public static RuntimeException InvalidArgumentType()
        {
            return new RuntimeException("Неверный тип аргумента");
        }

        public static RuntimeException InvalidArgumentValue()
        {
            return new RuntimeException("Неверное значение аргумента");
        }

        public static RuntimeException ComparisonNotSupportedException()
        {
            return new RuntimeException("Сравнение на больше/меньше для данного типа не поддерживается");
        }

        public static RuntimeException IndexedAccessIsNotSupportedException()
        {
            return new RuntimeException("Объект не поддерживает доступ по индексу");
        }

        public static RuntimeException IteratorIsNotDefined()
        {
            return new RuntimeException("Итератор не определен");
        }

        public static RuntimeException UseProcAsAFunction()
        {
            return new RuntimeException("Использование процедуры, как функции");
        }

        public static RuntimeException DivideByZero()
        {
            return new RuntimeException("Деление на ноль");
        }


    }

    public class ExternalSystemException : RuntimeException
    {
        public ExternalSystemException(Exception reason)
            : base("Внешнее исключение", reason)
        {
        }

        public override string Message
        {
            get
            {
                string innerMessage = InnerException == null? "" : "\n" + InnerException.Message;
                return base.Message + innerMessage;
            }
        }

    }

    public class WrongStackConditionException : RuntimeException
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

    public class ScriptInterruptionException : RuntimeException
    {
        public ScriptInterruptionException(int exitCode) : base("Script interrupted")
        {
            ExitCode = exitCode;
        }

        public int ExitCode { get; private set; }
    }

}
