/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Types;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Служебный класс, создаваемый конструктором объекта "ИнформацияОбОшибке".
    /// Превращается в полноценный объект ИнформацияОбОшибке в момент выброса исключения.
    /// 
    /// Данный класс предназначен для создания параметризованных исключений.
    /// </summary>
    /// <example>ВызватьИсключение Новый ИнформацияОбОшибке("Текст ошибки", ДополнительныеДанные);</example>
    [ContextClass("ИнформацияОбОшибкеШаблон", "ExceptionInfoTemplate", TypeUUID = "E0EDED59-D37A-42E7-9796-D6C061934B5D")]
    public class ExceptionTemplate : ContextIValueImpl
    {
        private static readonly TypeDescriptor _objectType = typeof(ExceptionTemplate).GetTypeFromClassMarkup();
        
        public string Message { get; private set; }
        public IValue Parameter { get; private set; }
        
        public ExceptionTemplate(string msg, IValue parameter) : base(_objectType)
        {
            this.Message = msg;
            this.Parameter = parameter;
        }
    }
}
