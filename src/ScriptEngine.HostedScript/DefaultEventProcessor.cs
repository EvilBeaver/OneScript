/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
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
            public Action<IBslProcess, IValue[]> Method;
        }

        private class HandlersList : IEnumerable<Handler>
        {
            private readonly List<Handler> _handlers = new List<Handler>();
            
            public void Add(ScriptDrivenObject target, string methodName)
            {
                var exist = _handlers.Exists(x => ReferenceEquals(x.Target, target) && String.Equals(x.MethodName, methodName, StringComparison.InvariantCultureIgnoreCase));
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
                _handlers.RemoveAll(x => ReferenceEquals(x.Target, target) && String.Equals(x.MethodName, methodName, StringComparison.InvariantCultureIgnoreCase));
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
        
        private readonly Dictionary<IRuntimeContextInstance, Dictionary<string, HandlersList>> _registeredHandlers 
            = new Dictionary<IRuntimeContextInstance, Dictionary<string, HandlersList>>();

        private readonly object _subscriptionLock = new object();
        
        public void AddHandler(
            IRuntimeContextInstance eventSource,
            string eventName,
            IRuntimeContextInstance handlerTarget,
            string handlerMethod)
        {
            if (!(handlerTarget is ScriptDrivenObject handlerScript))
                throw RuntimeException.InvalidArgumentType("handlerTarget");

            lock (_subscriptionLock)
            {
                if (!_registeredHandlers.TryGetValue(eventSource, out var handlers))
                {
                    handlers = new Dictionary<string, HandlersList>();
                    _registeredHandlers[eventSource] = handlers;
                }

                if (!handlers.TryGetValue(eventName, out var handlersList))
                {
                    handlersList = new HandlersList();
                    handlers[eventName] = handlersList;
                }

                handlersList.Add(handlerScript, handlerMethod);
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
            
            lock (_subscriptionLock)
            {
                if (!_registeredHandlers.TryGetValue(eventSource, out var handlers))
                {
                    return;
                }

                if (handlers.TryGetValue(eventName, out var handlersList))
                {
                    handlersList.Remove(handlerScript, handlerMethod);
                }
            }
        }

        public void HandleEvent(IRuntimeContextInstance eventSource, string eventName, IValue[] eventArgs,
            IBslProcess process)
        {
            HandlersList handlersLocalCopy;

            lock (_subscriptionLock)
            {

                if (!_registeredHandlers.TryGetValue(eventSource, out var handlers))
                    return;

                if (!handlers.TryGetValue(eventName, out handlersLocalCopy))
                {
                    return;
                }
            }

            foreach (var handler in handlersLocalCopy)
            {
                handler.Method(process, eventArgs);
            }
        }
    }
}