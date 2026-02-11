using System;

namespace ScriptEngine.Serialization
{
    public class SourceCodeRestorationException : Exception
    {
        public SourceCodeRestorationException(string message) : base(message)
        {
        }

        public SourceCodeRestorationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
