using System;

namespace ScriptEngine.Serialization
{
    public class CodeSourceImageException : Exception
    {
        public CodeSourceImageException()
        {
        }

        public CodeSourceImageException(string message) : base(message)
        {
        }

        public CodeSourceImageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
