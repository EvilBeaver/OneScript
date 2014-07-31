using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public class EngineException : ApplicationException
    {
        public EngineException()
        {

        }
        public EngineException(string message) : base(message)
        {

        }

        public EngineException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }

    public class TypeConversionException : EngineException
    {
        private TypeConversionException(string message) : base(message)
        {

        }

        public static TypeConversionException ConvertToNumberException()
        {
            return new TypeConversionException("Преобразование к типу 'Число' не может быть выполнено");
        }

        public static TypeConversionException ConvertToDateException()
        {
            return new TypeConversionException("Преобразование к типу 'Дата' не может быть выполнено");
        }

        public static TypeConversionException ConvertToBooleanException()
        {
            return new TypeConversionException("Преобразование к типу 'Булево' не может быть выполнено");
        }
        public static TypeConversionException ConvertToObjectException()
        {
            return new TypeConversionException("Значение не является значением объектного типа");
        }
    }
}
