/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Localization;

namespace OneScript.Exceptions
{
    public class RuntimeException : BslCoreException
    {
        public RuntimeException(BilingualString message, Exception innerException) : base(message, innerException)
        {
        }
        
        public RuntimeException(BilingualString message) : base(message)
        {
        }

        public RuntimeException(string message_ru, string message_en)
            : base(new BilingualString(message_ru, message_en))
        {
        }

        #region Static Factory Methods

        public static RuntimeException DeprecatedMethodCall(string name)
        {
            return new RuntimeException(
                $"Вызов безнадёжно устаревшего метода {name}",
                $"Call to deprecated method {name}");
        }

        public static RuntimeException MethodNotFoundException(string methodName)
        {
            return new RuntimeException(
                $"Метод объекта не обнаружен ({methodName})",
                $"Method not found ({methodName})");
        }

        public static RuntimeException MethodNotFoundException(string methodName, string objectName)
        {
            return new RuntimeException(
                $"Метод объекта не обнаружен ({{{objectName}}}::{methodName})",
                $"Method not found ({{{objectName}}}::{methodName})");
        }

        public static RuntimeException TooManyArgumentsPassed()
        {
            return new RuntimeException(
                "Слишком много фактических параметров",
                "Too many arguments were passed");
        }

        public static RuntimeException TooFewArgumentsPassed()
        {
            return new RuntimeException(
                "Недостаточно фактических параметров",
                "Too few arguments were passed");
        }
        
        public static RuntimeException MissedArgument()
        {
            return new RuntimeException(
                "Пропущен обязательный параметр",
                "Missed mandatory argument");
        }

        public static RuntimeException InvalidArgumentType()
        {
            return new RuntimeException(
                "Неверный тип аргумента", 
                "Invalid type of argument");
        }

        public static RuntimeException InvalidArgumentType(string argName)
        {
            return new RuntimeException(
                $"Неверный тип аргумента '{argName}'", 
                $"Invalid type of argument '{argName}'");
        }

        public static RuntimeException InvalidArgumentType(int argNum, string argName )
        {
            return new RuntimeException(
                $"Неверный тип аргумента номер {argNum} '{argName}'", 
                $"Invalid type of argument number {argNum} '{argName}'");
        }

        public static RuntimeException InvalidNthArgumentType(int argNum)
        {
            return new RuntimeException(
                $"Неверный тип аргумента номер {argNum}", 
                $"Invalid type of argument number {argNum}");
        }
        
        public static RuntimeException InvalidArgumentValue()
        {
            return new RuntimeException(
                "Неверное значение аргумента", 
                "Invalid argument value");
        }

        public static RuntimeException InvalidArgumentValue(object value)
        {
            return new RuntimeException(
                $"Неверное значение аргумента {value}", 
                $"Invalid value for argument {value}");
        }

        public static RuntimeException InvalidNthArgumentValue(int argNum)
        {
            return new RuntimeException(
                $"Неверное значение аргумента номер {argNum}", 
                $"Invalid value for argument number {argNum}");
        }
        
        public static RuntimeException ComparisonNotSupportedException()
        {
            return new RuntimeException(
                "Сравнение на больше/меньше для данного типа не поддерживается",
                "Greater-than/Less-than comparison operations are not supported");
        }
        
        public static RuntimeException ComparisonNotSupportedException(string type1, string type2)
        {
            return new RuntimeException(
                $"Сравнение на больше/меньше для данного типа не поддерживается {type1} <-> {type2}",
                $"Greater-than/Less-than comparison operations are not supported for {type1} <-> {type2}");
        }

        public static RuntimeException IndexedAccessIsNotSupportedException()
        {
            return new RuntimeException(
                "Объект не поддерживает доступ по индексу",
                "Indexed access is not supported");
        }

        public static RuntimeException IteratorIsNotDefined()
        {
            return new RuntimeException("Итератор не определен","Iterator is not defined");
        }

        public static RuntimeException UseProcAsAFunction()
        {
            return new RuntimeException(
                "Использование процедуры как функции",
                "Procedure called as function");
        }

        public static RuntimeException DivideByZero()
        {
            return new RuntimeException("Деление на ноль", "Divide by zero");
        }

        public static RuntimeException ConstructorNotFound(string typeName)
        {
            return new RuntimeException(
                $"Конструктор не найден ({typeName})",
                $"Constructor not found ({typeName})");
        }
        
        public static RuntimeException TypeIsNotDefined(string typeName)
        {
            return new RuntimeException(
                $"Тип не определен. Конструктор не найден ({typeName})", // для совместимости с v1
                $"Type is not defined ({typeName})");
        }
        
        public static RuntimeException TypeIsNotRegistered(string typeName)
        {
            return new RuntimeException(
                $"Тип не зарегистрирован ({typeName})",
                $"Type is not registered ({typeName})");
        }
        
        public static RuntimeException InvalidEncoding(string encoding)
        {
            return new RuntimeException(
                $"Неправильное имя кодировки '{encoding}'",
                $"Invalid encoding name '{encoding}'");
        }
        
        #endregion
    }
}