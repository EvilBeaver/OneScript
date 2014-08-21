#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    [ContextClass("BasicArray")]
    public class SafeArrayWrapper : ContextIValueImpl, ICollectionContext, IObjectWrapper
    {
        private object[] _array;

        public SafeArrayWrapper(object safearray)
        {
            _array = (object[])safearray;
        }

        public SafeArrayWrapper(object[] safearray)
        {
            _array = safearray;
        }

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

        public override IValue GetIndexedValue(IValue index)
        {
            var intIndex = (int)index.AsNumber();
            return COMWrapperContext.CreateIValue(_array[intIndex]);
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            var intIndex = (int)index.AsNumber();
            var newValue = COMWrapperContext.MarshalIValue(val);
            _array[intIndex] = newValue;
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
#endif