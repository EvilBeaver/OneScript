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
            _exc = source;
        }

        [ContextProperty("Описание", "Description")]
        public string Message 
        { 
            get { return _exc.Message; } 
        }

        [ContextProperty("Причина", "Cause")]
        public ExceptionInfoContext InnerException
        {
            get 
            {
                if (_exc != null)
                    return new ExceptionInfoContext(_exc.InnerException);
                else
                    return null;
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
