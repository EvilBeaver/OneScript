/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.Exceptions;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Коллекция элементов стека вызовов. Доступен обход с помощью Для Каждого...Из
    /// Содержит объекты типа КадрСтекаВызовов
    /// </summary>
    [ContextClass("КоллекцияКадровСтекаВызовов", "CallStackFramesCollection")]
    public class StackTraceCollectionContext : AutoCollectionContext<StackTraceCollectionContext, StackTraceItemContext>
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

        /// <summary>
        /// Возвращает количество кадров в стеке вызовов
        /// </summary>
        /// <returns>Число - Количество кадров в стеке вызовов</returns>
        [ContextMethod("Количество", "Count")]
        public override int Count()
        {
            return _frames.Count;
        }

        public override IEnumerator<StackTraceItemContext> GetEnumerator()
        {
            return _frames.GetEnumerator();
        }

        public override bool IsIndexed => true;

        public override IValue GetIndexedValue(IValue index)
        {
            var idx = (int)index.AsNumber();

            if (idx < 0 || idx >= Count())
                throw IndexOutOfBoundsException();

            return _frames[idx];
        }

        private static RuntimeException IndexOutOfBoundsException()
        {
            return new RuntimeException("Значение индекса выходит за пределы диапазона");
        }

    }
}
