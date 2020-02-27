/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine.Contexts;

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

        void HandleEvent(IRuntimeContextInstance eventSource, string eventName, IValue[] eventArgs);
    }
}