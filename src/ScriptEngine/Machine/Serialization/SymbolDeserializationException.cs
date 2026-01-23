using System;

namespace ScriptEngine.Machine.Serialization
{
    public class SymbolDeserializationException : Exception
    {
        public SymbolDeserializationException()
        {
        }

        public SymbolDeserializationException(string message) : base(message)
        {
        }

        public SymbolDeserializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
