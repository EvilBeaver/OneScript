/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Execution;

namespace ScriptEngine.Machine
{
    public interface IEventProcessor
    {
        void AddHandler(
            IRuntimeContextInstance eventSource,
            string eventName,
            IRuntimeContextInstance handlerTarget,
            string handlerMethod);
        
        void RemoveHandler(
            IRuntimeContextInstance eventSource,
            string eventName,
            IRuntimeContextInstance handlerTarget,
            string handlerMethod);

        void HandleEvent(IRuntimeContextInstance eventSource, string eventName, IValue[] eventArgs, IBslProcess process);

        /// <summary>
        /// Снимает все подписки на события указанного источника.
        ///
        /// Нужен источникам, которые живут меньше самого процессора событий: без этого реестр
        /// подписок удерживает такой источник до конца работы движка. Реализация по умолчанию
        /// ничего не делает, чтобы не ломать сторонние процессоры событий.
        /// </summary>
        /// <param name="eventSource">Источник, подписки на который нужно снять.</param>
        void RemoveAllHandlers(IRuntimeContextInstance eventSource)
        {
        }
    }
}