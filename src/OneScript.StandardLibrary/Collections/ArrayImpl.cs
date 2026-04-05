/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;

namespace OneScript.StandardLibrary.Collections
{
    [ContextClass("Массив", "Array")]
    public class ArrayImpl : AutoCollectionContext<ArrayImpl, IValue>, IValueArray
    {
        private readonly List<IValue> _values;

        public ArrayImpl()
        {
            _values = new List<IValue>();
        }
        
        public ArrayImpl(int size)
        {
            _values = new List<IValue>(size);
        }

        public ArrayImpl(IEnumerable<IValue> values)
        {
            _values = new List<IValue>(values);
        }

        public override bool IsIndexed => true;

        #region Native Runtime Bridge
        
        public BslValue this[int index]
        {
            get => (BslValue)_values[index];
            set => _values[index] = value;
        }
        
        #endregion
        
        public override IValue GetIndexedValue(IValue index)
        {
            if(index.SystemType == BasicTypes.Number)
                return Get((int)index.AsNumber());

            return base.GetIndexedValue(index);
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.SystemType == BasicTypes.Number)
                Set((int)index.AsNumber(), val);
            else
                base.SetIndexedValue(index, val);
        }

        #region ICollectionContext Members
        
        [ContextMethod("Количество", "Count")]
        public override int Count()
        {
            return _values.Count;
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _values.Clear();
        }

        #endregion

        #region IEnumerable<IRuntimeContextInstance> Members

        public override IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _values)
            {
                yield return item;
            }
        }

        #endregion

        [ContextMethod("Добавить", "Add")]
        public void Add(IValue value = null)
        {
           _values.Add(value ?? ValueFactory.Create());
        }

        [ContextMethod("Вставить", "Insert")]
        public void Insert(int index, IValue value = null)
        {
            if (index < 0)
                throw RuntimeException.IndexOutOfRange();

            if (index > _values.Count)
                Extend(index - _values.Count);

            _values.Insert(index, value ?? ValueFactory.Create());
        }

        [ContextMethod("Найти", "Find")]
        public IValue Find(IValue what)
        {
            var idx = _values.FindIndex(x => x.StrictEquals(what));
            return idx>=0 ? ValueFactory.Create(idx) : ValueFactory.Create();    
        }

        [ContextMethod("Удалить", "Delete")]
        public void Remove(int index)
        {
            if (index < 0 || index >= _values.Count)
                throw RuntimeException.IndexOutOfRange();

            _values.RemoveAt(index);
        }

        [ContextMethod("ВГраница", "UBound")]
        public int UpperBound()
        {
            return _values.Count - 1;
        }

        [ContextMethod("Получить", "Get")]
        public IValue Get(int index)
        {
            if (index < 0 || index >= _values.Count)
                throw RuntimeException.IndexOutOfRange();

            return _values[index];
        }

        [ContextMethod("Установить", "Set")]
        public void Set(int index, IValue value)
        {
            if (index < 0 || index >= _values.Count)
                throw RuntimeException.IndexOutOfRange();

            _values[index] = value;
        }

        private void Extend(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                _values.Add(ValueFactory.Create());
            }
        }

        private static ArrayImpl CreateArray(int bound)
        {
            var array = new ArrayImpl(bound);
            for (int i = 0; i < bound; i++)
            {
                array._values.Add(ValueFactory.Create());
            }
            return array;
        }

        private static ArrayImpl CloneArray(ArrayImpl cloneable)
        {
            ArrayImpl clone = new ArrayImpl(cloneable._values.Count);
            foreach (var item in cloneable._values)
            {
                clone._values.Add(item is ArrayImpl arr ? CloneArray(arr) : item );
            }
            return clone;
        }

        [ScriptConstructor]
        public static ArrayImpl Constructor()
        {
            return new ArrayImpl();
        }

        /// <summary>
        /// Позволяет задать измерения массива при его создании
        /// </summary>
        /// <param name="dimensions">Числовые размерности массива. Например, "Массив(2,3)", создает двумерный массив 2х3.</param>
        /// <returns></returns>
        [ScriptConstructor(Name = "По количеству элементов")]
        public static ArrayImpl Constructor(IValue[] dimensions)
        {
            if (dimensions.Length == 1 && dimensions[0] is FixedArrayImpl fa)
            { 
                    return Constructor(fa);
            }

            // fail fast
            int size = 0;
            for (int dim = 0; dim < dimensions.Length; dim++)
            {
                if (dimensions[dim] == null)
                    throw RuntimeException.InvalidNthArgumentType(dim + 1);

                size = (int)dimensions[dim].AsNumber();
                if (size <= 0)
                    throw RuntimeException.InvalidNthArgumentValue(dim + 1);
            }

            var newInst = CreateArray(size); // длина по последней размерности

            for (int dim = dimensions.Length - 2; dim >= 0; dim--) // если размерность >= 2
            {
                ArrayImpl nested = newInst;
                int bound = (int)dimensions[dim].AsNumber();
 
                newInst = new ArrayImpl(bound);
                for (int i = 0; i < bound; i++)
                {
                    newInst._values.Add(CloneArray(nested));
                }
            }

            return newInst;
        }

        [ScriptConstructor(Name = "На основании фиксированного массива")]
        public static ArrayImpl Constructor(FixedArrayImpl fixedArray)
        {
            return new ArrayImpl(fixedArray);
        }
    }
}
