
namespace OneScript.Core
{
    public class TypeConversionException : EngineException
    {
        private TypeConversionException(string message)
            : base(message)
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

        public static TypeConversionException ComparisonIsNotSupportedException()
        {
            return new TypeConversionException("Сравнение на больше/меньше возможно только для совпадающих значений примитивных типов");
        }
    }
}