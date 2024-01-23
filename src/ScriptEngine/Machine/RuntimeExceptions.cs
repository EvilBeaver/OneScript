﻿/*----------------------------------------------------------
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
            return new TypeConvertionException("Число");
        }

        public static RuntimeException ConvertToBooleanException()
        {
            return new TypeConvertionException("Булево");
        }

        public static RuntimeException ConvertToDateException()
        {
            return new TypeConvertionException("Дата");
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

        public static RuntimeException MissedArgument()
        {
            return new RuntimeException("Пропущен обязательный параметр");
        }

        public static RuntimeException MissedNthArgument(int argNum)
        {
            return new RuntimeException($"Пропущен обязательный параметр номер {argNum}" );
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

        public static RuntimeException InvalidEncoding(string encoding)
        {
            return new RuntimeException($"Неправильное имя кодировки '{encoding}'");
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

    public class TypeConvertionException : RuntimeException
    {
        public TypeConvertionException(string typename) 
            : base($"Преобразование к типу '{typename}' не поддерживается")
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
