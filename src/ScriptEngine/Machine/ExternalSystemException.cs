using System;

namespace ScriptEngine.Machine
{
    public class ExternalSystemException : RuntimeException
    {
        public ExternalSystemException(Exception reason)
            : base(string.Format("Внешнее исключение ({0}): {1}", reason.GetType().FullName, reason.Message), reason)
        {
        }
    }
}