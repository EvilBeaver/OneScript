using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    [ContextClass("ИнформацияОбОшибке")]
    class ExceptionInfoContext : AutoContext<ExceptionInfoContext>
    {
        Exception _exc;
        public ExceptionInfoContext(Exception source)
        {
            _exc = source;
        }

        [ContextProperty("Описание")]
        public string Message 
        { 
            get { return _exc.Message; } 
        }

        [ContextProperty("Причина")]
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

        [ContextMethod("ПодробноеОписаниеОшибки")]
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
