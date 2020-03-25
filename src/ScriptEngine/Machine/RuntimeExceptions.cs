/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Language;

namespace ScriptEngine.Machine
{
    public class RuntimeException : ScriptException
    {
        private List<ExecutionFrameInfo> _frames;

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

        public IEnumerable<ExecutionFrameInfo> GetStackTrace()
        {
            return _frames.AsReadOnly();
        }

        internal IList<ExecutionFrameInfo> CallStackFrames => _frames;

        internal void InitCallStackFrames(IEnumerable<ExecutionFrameInfo> src)
        {
            _frames = src == null ? new List<ExecutionFrameInfo>() : new List<ExecutionFrameInfo>(src);
        }

        public static RuntimeException DeprecatedMethodCall(string name)
        {
            return new RuntimeException($"Вызов безнадёжно устаревшего метода {name}");
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
            return new RuntimeException($"Метод объекта не обнаружен ({methodName})");
        }

        public static RuntimeException MethodNotFoundException(string methodName, string objectName)
        {
            return new RuntimeException($"Метод объекта не обнаружен ({{{objectName}}}::{methodName})");
        }

        public static RuntimeException ValueIsNotObjectException()
        {
            return new RuntimeException("Значение не является значением объектного типа");
        }

        public static RuntimeException TooManyArgumentsPassed()
        {
            return new RuntimeException("Слишком много фактических параметров");
        }

        public static RuntimeException TooFewArgumentsPassed()
        {
            return new RuntimeException("Недостаточно фактических параметров");
        }

        public static RuntimeException InvalidArgumentType()
        {
            return new RuntimeException("Неверный тип аргумента");
        }

        public static RuntimeException InvalidArgumentType(string argName)
        {
            return new RuntimeException(String.Format("Неверный тип аргумента '{0}'", argName));
        }

        public static RuntimeException InvalidNthArgumentType(int argNum)
        {
            return new RuntimeException(String.Format("Неверный тип аргумента номер {0}", argNum));
        }

        public static RuntimeException InvalidArgumentType(int argNum, string argName )
        {
            return new RuntimeException(String.Format("Неверный тип аргумента номер {0} '{1}'", argNum, argName ));
        }

        public static RuntimeException InvalidArgumentValue()
        {
            return new RuntimeException("Неверное значение аргумента");
        }

        public static RuntimeException InvalidNthArgumentValue(int argNum)
        {
            return new RuntimeException(String.Format("Неверное значение аргумента номер {0}", argNum));
        }

        public static RuntimeException InvalidArgumentValue(object value)
        {
            return new RuntimeException("Неверное значение аргумента {"+value.ToString()+"}");
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
