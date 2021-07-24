/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript
{
    public class DefaultEventProcessor : IEventProcessor
    {
        private struct Handler
        {
            public ScriptDrivenObject Target;
            public string MethodName;
            public Action<IValue[]> Method;
        }

        private class HandlersList : IEnumerable<Handler>
        {
            private List<Handler> _handlers = new List<Handler>();
            
            public void Add(ScriptDrivenObject target, string methodName)
            {
                var exist = _handlers.Exists(x => ReferenceEquals(x.Target, target) && x.MethodName.ToLowerInvariant() == methodName.ToLowerInvariant());
                if (!exist)
                {
                    _handlers.Add(new Handler
                    {
                        Target = target,
                        MethodName = methodName,
                        Method = target.GetMethodExecutor(methodName)
                    });
                }
            }

            public void Remove(ScriptDrivenObject target, string methodName)
            {
                _handlers.RemoveAll(x => ReferenceEquals(x.Target, target) && x.MethodName.ToLowerInvariant() == methodName.ToLowerInvariant());
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
            if (!(handlerTarget is ScriptDrivenObject handlerScript))
                throw RuntimeException.InvalidArgumentType("handlerTarget");
            
            if (!_registeredHandlers.TryGetValue(eventSource, out var handlers))
            {
                handlers = new Dictionary<string, HandlersList>();
                handlers[eventName] = new HandlersList();
                _registeredHandlers[eventSource] = handlers;
            }
            
            handlers[eventName].Add(handlerScript, handlerMethod);
            if (eventSource is IEventSourceNotify notify)
            {
                notify.OnSubscribe(eventName, handlerScript);
            }
        }

        public void RemoveHandler(
            IRuntimeContextInstance eventSource,
            string eventName,
            IRuntimeContextInstance handlerTarget,
            string handlerMethod)
        {
            if (!(handlerTarget is ScriptDrivenObject handlerScript))
                throw RuntimeException.InvalidArgumentType("handlerTarget");
            
            if (_registeredHandlers.TryGetValue(eventSource, out var handlers))
            {
                handlers[eventName].Remove(handlerScript, handlerMethod);
                if (eventSource is IEventSourceNotify notify)
                {
                    notify.OnUnsubscribe(eventName, handlerScript);
                }
            }
        }

        public void HandleEvent(IRuntimeContextInstance eventSource, string eventName, IValue[] eventArgs)
        {
            if (!_registeredHandlers.TryGetValue(eventSource, out var handlers)) 
                return;
            
            foreach (var handler in handlers[eventName])
            {
                handler.Method(eventArgs);
            }
        }
    }
}