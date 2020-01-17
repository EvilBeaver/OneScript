/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__
using System;
using System.Collections.Generic;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Вспомогательный класс для работы с объектами COMSafeArray, получаемыми из COM-объектов.
    /// На данный момент класс не является полноценной заменой для COMSafeArray и его нельзя создать вручную.
    /// </summary>
    [ContextClass("SafeArrayWrapper")]
    public class SafeArrayWrapper : AutoContext<SafeArrayWrapper>, ICollectionContext, IObjectWrapper, IEnumerable<IValue>
    {
        private readonly object[] _array;

        public SafeArrayWrapper(object safearray)
        {
            _array = (object[])safearray;
        }

        public SafeArrayWrapper(object[] safearray)
        {
            _array = safearray;
        }

        [ContextMethod("Количество", "Count")]
		public int Count()
        {
            return _array.Length;
        }

        public override bool IsIndexed
        {
            get
            {
                return true;
            }
        }

        [ContextMethod("GetValue")]
        public IValue GetValue(int index)
        {
            return COMWrapperContext.CreateIValue(_array[index]);
        }

        [ContextMethod("SetValue")]
        public void SetValue(int index, IValue value)
        {
            var newValue = COMWrapperContext.MarshalIValue(value);
            _array[index] = newValue;
        }

        [ContextMethod("Выгрузить", "Unload")]
        public object Unload()
        {
            throw new NotSupportedException("FIXME: Method 'Unload' is not supported. Consider use SafeArrayWrapper as V8.Array directly.");
        }

        public override IValue GetIndexedValue(IValue index)
        {
            var intIndex = (int)index.AsNumber();
            return GetValue(intIndex);
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            var intIndex = (int)index.AsNumber();
            SetValue(intIndex, val);
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            for (int i = 0; i < _array.Length; i++)
            {
                yield return COMWrapperContext.CreateIValue(_array[i]);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public object UnderlyingObject
        {
            get { return _array; }
        }
    }
}
//#endif