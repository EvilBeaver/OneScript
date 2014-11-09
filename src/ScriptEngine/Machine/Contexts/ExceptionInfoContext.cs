using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    [ContextClass("ИнформацияОбОшибке", "ErrorInfo")]
    class ExceptionInfoContext : AutoContext<ExceptionInfoContext>
    {
        Exception _exc;
        public ExceptionInfoContext(Exception source)
        {
            if (source == null)
                throw new ArgumentNullException();

            _exc = source;
        }

        [ContextProperty("Описание", "Description")]
        public string Message 
        { 
            get { return _exc.Message; } 
        }

        [ContextProperty("Причина", "Cause")]
        public IValue InnerException
        {
            get 
            {
                if (_exc.InnerException != null)
                    return new ExceptionInfoContext(_exc.InnerException);
                else
                    return ValueFactory.Create();
            }
        }

        [ContextMethod("ПодробноеОписаниеОшибки", "DetailedDescription")]
        public string GetDescription()
        {
            return _exc.ToString();
        }

        public override string ToString()
        {
            return Message;
        }

    }
}
