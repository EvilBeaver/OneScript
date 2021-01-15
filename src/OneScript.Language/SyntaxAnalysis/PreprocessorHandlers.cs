/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public class PreprocessorHandlers : IDirectiveHandler, IEnumerable<IDirectiveHandler>
    {
        private readonly IList<IDirectiveHandler> _handlers;

        public PreprocessorHandlers()
        {
           _handlers = new List<IDirectiveHandler>();
        }
        
        public PreprocessorHandlers(IEnumerable<IDirectiveHandler> handlers)
        {
            _handlers = new List<IDirectiveHandler>(handlers);
        }
        
        public void Add(IDirectiveHandler handler)
        {
            _handlers.Add(handler);
        }
        
        public void Remove(IDirectiveHandler handler)
        {
            _handlers.Remove(handler);
        }
        
        public IDirectiveHandler Get(Type type)
        {
            return _handlers.FirstOrDefault(type.IsInstanceOfType);
        }
        
        public T Get<T>() where T : IDirectiveHandler
        {
            return (T)Get(typeof(T));
        }

        public PreprocessorHandlers Slice(Func<IDirectiveHandler, bool> predicate)
        {
            var slice = _handlers.Where(predicate).ToArray();
            if (slice.Length == 0)
                return default;
            
            return new PreprocessorHandlers(slice);
        }

        public void OnModuleEnter()
        {
            foreach (var handler in _handlers)
            {
                handler.OnModuleEnter();
            }
        }

        public void OnModuleLeave()
        {
            foreach (var handler in _handlers)
            {
                handler.OnModuleLeave();
            }
        }

        public bool HandleDirective(ref Lexem lexem, ILexer lexer)
        {
            foreach (var handler in _handlers)
            {
                if (handler.HandleDirective(ref lexem, lexer))
                    return true;
            }

            return false;
        }

        public IEnumerator<IDirectiveHandler> GetEnumerator()
        {
            return _handlers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _handlers).GetEnumerator();
        }
    }
}