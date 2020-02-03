/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript
{
    public class DefaultEventProcessor : IEventProcessor
    {
        private struct Handler
        {
            public IRuntimeContextInstance Target;
            public int MethodId;
        }

        private class HandlersList : IEnumerable<Handler>
        {
            private List<Handler> _handlers = new List<Handler>();
            
            public void Add(IRuntimeContextInstance target, string methodName)
            {
                var id = target.FindMethod(methodName);
                var exist = _handlers.Exists(x => ReferenceEquals(x.Target, target) && x.MethodId == id);
                if (!exist)
                {
                    _handlers.Add(new Handler
                    {
                        Target = target,
                        MethodId = id
                    });
                }
            }

            public void Remove(IRuntimeContextInstance target, string methodName)
            {
                var id = target.FindMethod(methodName);
                _handlers.RemoveAll(x => ReferenceEquals(x.Target, target) && x.MethodId == id);
            }

            public IEnumerator<Handler> GetEnumerator()
            {
                return _handlers.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        
        private Dictionary<IRuntimeContextInstance, Dictionary<string, HandlersList>> _registeredHandlers = new Dictionary<IRuntimeContextInstance, Dictionary<string, HandlersList>>();
        
        public void AddHandler(
            IRuntimeContextInstance eventSource,
            string eventName,
            IRuntimeContextInstance handlerTarget,
            string handlerMethod)
        {
            if (!_registeredHandlers.TryGetValue(eventSource, out var handlers))
            {
                handlers = new Dictionary<string, HandlersList>();
                handlers[eventName] = new HandlersList();
                _registeredHandlers[eventSource] = handlers;
            }
            
            handlers[eventName].Add(handlerTarget, handlerMethod);
        }

        public void RemoveHandler(
            IRuntimeContextInstance eventSource,
            string eventName,
            IRuntimeContextInstance handlerTarget,
            string handlerMethod)
        {
            if (_registeredHandlers.TryGetValue(eventSource, out var handlers))
            {
                handlers[eventName].Remove(handlerTarget, handlerMethod);
            }
        }

        public void HandleEvent(IRuntimeContextInstance eventSource, string eventName, IValue[] eventArgs)
        {
            if (!_registeredHandlers.TryGetValue(eventSource, out var handlers)) 
                return;
            
            foreach (var handler in handlers[eventName])
            {
                handler.Target.CallAsProcedure(handler.MethodId, eventArgs);
            }
        }
    }
}