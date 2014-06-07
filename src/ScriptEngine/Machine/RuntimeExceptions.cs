using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public class RuntimeException : ApplicationException
    {
        public RuntimeException() : base()
        {
        }

        public RuntimeException(string msg) : base(msg)
        {
        }

        public RuntimeException(string msg, Exception inner)
            : base(msg, inner)
        {
        }

        public static RuntimeException ConvertToNumberException()
        {
            return new RuntimeException("Conversion to Number is not supported");
        }

        public static RuntimeException ConvertToBooleanException()
        {
            return new RuntimeException("Conversion to Boolean is not supported");
        }

        public static RuntimeException ConvertToDateException()
        {
            return new RuntimeException("Conversion to Date is not supported");
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
            return new RuntimeException(string.Format("Object method {0} is not found", methodName));
        }

        public static RuntimeException ValueIsNotObjectException()
        {
            return new RuntimeException("Value is not an Object, thus it can't be used with 'dot' operator");
        }

        public static RuntimeException TooManyArgumentsPassed()
        {
            return new RuntimeException("Too many actual parameters have been passed");
        }

        public static RuntimeException TooLittleArgumentsPassed()
        {
            return new RuntimeException("Too litte actual parameters have been passed");
        }

        public static RuntimeException ArgHasNoDefaultValue(int argNum)
        {
            return new RuntimeException(string.Format("Argument {0} has no default value", argNum));
        }

        public static RuntimeException InvalidArgumentType()
        {
            return new RuntimeException("Invalid argument type");
        }

        public static RuntimeException InvalidArgumentValue()
        {
            return new RuntimeException("Invalid argument value");
        }

        public static RuntimeException ComparisonNotSupportedException()
        {
            return new RuntimeException("Comparison if less/greater is not supported on type specified");
        }

        public static RuntimeException IndexedAccessIsNotSupportedException()
        {
            return new RuntimeException("Object doesn't supported indexed access");
        }

        public static RuntimeException IteratorIsNotDefined()
        {
            return new RuntimeException("Object doesn't support iterations");
        }

        public static RuntimeException UseProcAsAFunction()
        {
            return new RuntimeException("Using procedure as a function");
        }

        public static RuntimeException DivideByZero()
        {
            return new RuntimeException("Division by zero");
        }


    }

    public class ExternalSystemException : RuntimeException
    {
        public ExternalSystemException(Exception reason)
            : base("System exception", reason)
        {

        }
    }

    public class WrongStackConditionException : RuntimeException
    {
        public WrongStackConditionException()
            : base("Internal error: wrong stack condition")
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
            return new PropertyAccessException(string.Format("Object property {0} is not readable", prop));
        }

        public static PropertyAccessException GetPropIsNotWritableException(string prop)
        {
            return new PropertyAccessException(string.Format("Object property {0} is not writable", prop));
        }

        public static PropertyAccessException GetPropNotFoundException(string prop)
        {
            return new PropertyAccessException(string.Format("Object property {0} is not found", prop));
        }

    }

    public class ScriptInterruptionException : RuntimeException
    {
        public ScriptInterruptionException(int exitCode) : base("Script interupted")
        {
            ExitCode = exitCode;
        }

        public int ExitCode { get; private set; }
    }

}
