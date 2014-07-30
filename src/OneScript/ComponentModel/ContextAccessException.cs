using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.ComponentModel
{
    public class ContextAccessException : EngineException
    {
        public ContextAccessException(string message) : base(message)
        {
        }

        public static ContextAccessException PropNotFound(string name)
        {
            return new ContextAccessException("Свойство объекта не обнаружено: " + name);
        }

        public static ContextAccessException MethodNotFound(string name)
        {
            return new ContextAccessException("Метод объекта не обнаружен: " + name);
        }

        public static ContextAccessException PropIsNotReadable(string name)
        {
            return new ContextAccessException("Свойство объекта недоступно для чтения: " + name);
        }

        public static ContextAccessException PropIsNotReadable()
        {
            return new ContextAccessException("Свойство объекта недоступно для чтения");
        }

        public static ContextAccessException PropIsNotWritable(string name)
        {
            return new ContextAccessException("Свойство объекта недоступно для записи: " + name);
        }
    }
}
