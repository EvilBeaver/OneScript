using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Класс позволяет узнать информацию о произошедшем исключении.
    /// </summary>
    [ContextClass("ИнформацияОбОшибке", "ErrorInfo")]
    public class ExceptionInfoContext : AutoContext<ExceptionInfoContext>
    {
        ScriptException _exc;
        public ExceptionInfoContext(ScriptException source)
        {
            if (source == null)
                throw new ArgumentNullException();

            _exc = source;
        }

        /// <summary>
        /// Содержит краткое описание ошибки. Эквивалент Exception.Message в C#
        /// </summary>
        [ContextProperty("Описание", "Description")]
        public string Description 
        { 
            get { return _exc.ErrorDescription; } 
        }

        public string MessageWithoutCodeFragment
        {
            get { return _exc.MessageWithoutCodeFragment; }
        }

        public string DetailedDescription
        {
            get { return _exc.Message; }
        }

        /// <summary>
        /// Содержит вложенное исключение, если таковое было. Эквивалент Exception.InnerException в C#
        /// </summary>
        [ContextProperty("Причина", "Cause")]
        public IValue InnerException
        {
            get 
            {
                if (!(_exc is ExternalSystemException) && _exc.InnerException != null)
                {
                    ScriptException inner;
                    inner = _exc.InnerException as ScriptException;
                    if(inner == null)
                    {
                        inner = new ExternalSystemException(_exc.InnerException);
                    }
                    
                    return new ExceptionInfoContext(inner);
                }
                else
                    return ValueFactory.Create();
            }
        }

        /// <summary>
        /// Содержит подробное описание исключения, включая стек вызовов среды исполнения CLR.
        /// т.е. не стек вызовов скрипта, а стек вызовов скриптового движка.
        /// Эквивалентно функции Exception.ToString() в C#.
        /// </summary>
        /// <returns>Строка.</returns>
        [ContextMethod("ПодробноеОписаниеОшибки", "DetailedDescription")]
        public string GetDescription()
        {
            return _exc.ToString();
        }

        public override string ToString()
        {
            return Description;
        }

    }
}
