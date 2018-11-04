/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("Массив", "Array")]
    public class ArrayImpl : AutoContext<ArrayImpl>, ICollectionContext, IEnumerable<IValue>
    {
        private readonly List<IValue> _values;

        public ArrayImpl()
        {
            _values = new List<IValue>();
        }

        public ArrayImpl(IEnumerable<IValue> values)
        {
            _values = new List<IValue>(values);
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
            if(index.DataType == DataType.Number)
                return Get((int)index.AsNumber());

            return base.GetIndexedValue(index);
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.DataType == DataType.Number)
                _values[(int)index.AsNumber()] = val;
            else
                base.SetIndexedValue(index, val);
        }

        public override bool IsPropReadable(int propNum)
        {
            throw new NotImplementedException();
        }

        public override bool IsPropWritable(int propNum)
        {
            throw new NotImplementedException();
        }

        #region ICollectionContext Members
        
        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _values.Count;
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _values.Clear();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        #endregion

        #region IEnumerable<IRuntimeContextInstance> Members

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _values)
            {
                yield return item;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        [ContextMethod("Добавить", "Add")]
        public void Add(IValue value)
        {
            _values.Add(value);
        }

        [ContextMethod("Вставить", "Insert")]
        public void Insert(int index, IValue value)
        {
            _values.Insert(index, value);
        }

        [ContextMethod("Найти", "Find")]
        public IValue Find(IValue what)
        {
            var idx = _values.FindIndex(x => x.Equals(what));
            if(idx < 0)
            {
                return ValueFactory.Create();
            }
            else
            {
                return ValueFactory.Create(idx);
            }
        }

        [ContextMethod("Удалить", "Delete")]
        public void Remove(int index)
        {
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
            return _values[index];
        }

        [ContextMethod("Установить", "Set")]
        public void Set(int index, IValue value)
        {
            _values[index] = value;
        }

        private static void FillArray(ArrayImpl currentArray, int bound)
        {
            for (int i = 0; i < bound; i++)
            {
                currentArray._values.Add(ValueFactory.Create());
            }
        }

        private static IValue CloneArray(ArrayImpl cloneable)
        {
            ArrayImpl clone = new ArrayImpl();
            foreach (var item in cloneable._values)
            {
                if (item.DataType == DataType.Undefined)
                    clone._values.Add(ValueFactory.Create());
                else
                    clone._values.Add(item);
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
            ArrayImpl cloneable = null;
            for (int dim = dimensions.Length - 1; dim >= 0; dim--)
            {
                int bound = (int)dimensions[dim].AsNumber();
                var newInst = new ArrayImpl();
                FillArray(newInst, bound);
                if(cloneable != null)
                {
                    for (int i = 0; i < bound; i++)
                    {
                        newInst._values[i] = CloneArray(cloneable);
                    }
                }
                cloneable = newInst;
                
            }

            return cloneable;

        }

    }
}
