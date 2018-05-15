/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Коллекция элементов стека вызовов. Доступен обход с помощью Для Каждого...Из
    /// Содержит объекты типа КадрСтекаВызовов
    /// </summary>
    [ContextClass("КоллекцияКадровСтекаВызовов", "CallStackFramesCollection")]
    public class StackTraceCollectionContext : AutoContext<StackTraceCollectionContext>, ICollectionContext
    {
        private List<StackTraceItemContext> _frames;

        internal StackTraceCollectionContext(IEnumerable<ExecutionFrameInfo> frames)
        {
            _frames = frames.Select(x => new StackTraceItemContext()
            {
                ModuleName = x.Source,
                Method = x.MethodName,
                LineNumber = x.LineNumber
            }).ToList();
        }

        public int Count()
        {
            return _frames.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(_frames.GetEnumerator());
        }
    }
}
