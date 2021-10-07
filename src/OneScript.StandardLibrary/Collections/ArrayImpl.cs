/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections
{
    [ContextClass("Массив", "Array")]
    public class ArrayImpl : AutoCollectionContext<ArrayImpl, IValue>
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
            if (value == null)
                _values.Add(ValueFactory.Create());
            else 
                _values.Add(value);
        }

        [ContextMethod("Вставить", "Insert")]
        public void Insert(int index, IValue value = null)
        {
            if (index < 0)
                throw IndexOutOfBoundsException();

            if (index > _values.Count)
                Extend(index - _values.Count);

            if (value == null)
                _values.Insert(index, ValueFactory.Create());
            else
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
            if (index < 0 || index >= _values.Count)
                throw IndexOutOfBoundsException();

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
                throw IndexOutOfBoundsException();

            return _values[index];
        }

        [ContextMethod("Установить", "Set")]
        public void Set(int index, IValue value)
        {
            if (index < 0 || index >= _values.Count)
                throw IndexOutOfBoundsException();

            _values[index] = value;
        }

        private void Extend(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                _values.Add(ValueFactory.Create());
            }
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
                clone._values.Add(item ?? ValueFactory.Create());
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
            if (dimensions.Length == 1 && dimensions[0].GetRawValue() is FixedArrayImpl)
            {
                return Constructor(dimensions[0]);
            }
            
            ArrayImpl cloneable = null;
            for (int dim = dimensions.Length - 1; dim >= 0; dim--)
            {
                if (dimensions[dim] == null)
                    throw RuntimeException.InvalidNthArgumentType(dim + 1);

                int bound = (int)dimensions[dim].AsNumber();
                if (bound <= 0)
                    throw RuntimeException.InvalidNthArgumentValue(dim + 1);

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

        [ScriptConstructor(Name = "На основании фиксированного массива")]
        public static ArrayImpl Constructor(IValue fixedArray)
        {
            if (!(fixedArray.GetRawValue() is FixedArrayImpl val))
                throw RuntimeException.InvalidArgumentType();
            
            return new ArrayImpl(val);
        }

        private static RuntimeException IndexOutOfBoundsException()
        {
            return new RuntimeException("Значение индекса выходит за пределы диапазона");
        }
    }
}
