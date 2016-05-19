using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Служебный класс, создаваемый конструктором объекта "ИнформацияОбОшибке".
    /// Превращается в полноценный объект ИнформацияОбОшибке в момент выброса исключения.
    /// 
    /// Данный класс предназначен для создания параметризованных исключений.
    /// </summary>
    /// <example>ВызватьИсключение Новый ИнформацияОбОшибке("Текст ошибки", ДополнительныеДанные);</example>
    [ContextClass("ИнформацияОбОшибкеШаблон", "ExceptionInfoTemplate")]
    public class ExceptionTemplate : LibraryContextBase
    {
        public string Message { get; private set; }
        public IValue Parameter { get; private set; }

        public ExceptionTemplate(string msg, IValue parameter)
        {
            this.Message = msg;
            this.Parameter = parameter;
        }
    }
}
