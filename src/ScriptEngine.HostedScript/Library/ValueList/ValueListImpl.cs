/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.ValueList
{
    /// <summary>
    /// Стандартная универсальная коллекция системы 1С:Предприятие 8
    /// </summary>
    [ContextClass("СписокЗначений", "ValueList")]
    public class ValueListImpl : AutoContext<ValueListImpl>, ICollectionContext, IEnumerable<ValueListItem>
    {
        readonly List<ValueListItem> _items;
        public ValueListImpl()
        {
            _items = new List<ValueListItem>();
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
            if (index.DataType == Machine.DataType.Number)
            {
                return GetValue(index);
            }
            else
            {
                return base.GetIndexedValue(index);
            }
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.DataType == Machine.DataType.Number)
            {
                throw new RuntimeException("Индексированное значение доступно только для чтения");
            }
            else
            {
                base.SetIndexedValue(index, val);
            }
        }

        [ContextMethod("Получить", "Get")]
        public ValueListItem GetValue(IValue index)
        {
            int numericIndex = (int)index.AsNumber();
            return _items[numericIndex];
        }

        [ContextMethod("Добавить", "Add")]
        public ValueListItem Add(IValue value, string presentation = null, bool check = false, IValue picture = null)
        {
            var newItem = CreateNewListItem(value, presentation, check, picture);

            _items.Add(newItem);
            return newItem;
        }

        [ContextMethod("Вставить", "Insert")]
        public ValueListItem Insert(int index, IValue value, string presentation = null, bool check = false, IValue picture = null)
        {
            var newItem = CreateNewListItem(value, presentation, check, picture);
            _items.Insert(index, newItem);
            
            return newItem;
        }

        private static ValueListItem CreateNewListItem(IValue value, string presentation, bool check, IValue picture)
        {
            var newItem = new ValueListItem();
            newItem.Value = value;
            newItem.Presentation = presentation;
            newItem.Check = check;
            newItem.Picture = picture;
            return newItem;
        }

        [ContextMethod("ВыгрузитьЗначения", "UnloadValues")]
        public ArrayImpl UnloadValues()
        {
            return new ArrayImpl(_items.Select(x=>x.Value));
        }

        [ContextMethod("ЗагрузитьЗначения", "LoadValues")]
        public void LoadValues(ArrayImpl source)
        {
            Clear();
            _items.AddRange(source.Select(x => new ValueListItem() { Value = x }));
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _items.Clear();
        }

        [ContextMethod("ЗаполнитьПометки", "FillChecks")]
        public void FillChecks(bool check)
        {
            foreach (var item in _items)
            {
                item.Check = check;
            }
        }

        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(ValueListItem item)
        {
            return _items.IndexOf(item);
        }

        [ContextMethod("НайтиПоЗначению", "FindByValue")]
        public IValue FindByValue(IValue val)
        {
            var item = _items.FirstOrDefault(x => x.Value.Equals(val));
            if(item == null)
                return ValueFactory.Create();

            return item;
        }

        private int IndexByValue(IValue item)
        {
            item = item.GetRawValue();

            int index;

            if (item is ValueListItem)
            {
                index = IndexOf(item as ValueListItem);
                if (index == -1)
                    throw new RuntimeException("Элемент не принадлежит списку значений");
            }
            else
            {
                try
                {
                    index = decimal.ToInt32(item.AsNumber());
                }
                catch (RuntimeException)
                {
                    throw RuntimeException.InvalidArgumentType();
                }

                if (index < 0 || index >= _items.Count())
                    throw new RuntimeException("Значение индекса выходит за пределы диапазона");
            }

            return index;
        }

        [ContextMethod("Сдвинуть", "Move")]
        public void Move(IValue item, int direction)
        {
            int index_source = IndexByValue(item);

            int index_dest = index_source + direction;

            if (index_dest < 0 || index_dest >= _items.Count())
                throw RuntimeException.InvalidNthArgumentValue(2);

            ValueListItem itemObject = _items[index_source];

            if (index_source < index_dest)
            {
                _items.Insert(index_dest + 1, itemObject);
                _items.RemoveAt(index_source);
            }
            else
            {
                _items.RemoveAt(index_source);
                _items.Insert(index_dest, itemObject);
            }
        }

        [ContextMethod("Скопировать", "Copy")]
        public ValueListImpl Copy()
        {
            var newList = new ValueListImpl();
            foreach (var item in _items)
            {
                newList.Add(item.Value, item.Presentation, item.Check, item.Picture);
            }

            return newList;
        }

        [ContextMethod("СортироватьПоЗначению", "SortByValue")]
        public void SortByValue(SortDirectionEnum? direction = null)
        {
            if (direction == null || direction == SortDirectionEnum.Asc)
            {
                _items.Sort((x, y) => SafeCompare(x.Value, y.Value));
            }
            else
            {
                _items.Sort((x, y) => SafeCompare(y.Value, x.Value));
            }
        }

        private int SafeCompare(IValue x, IValue y)
        {
            try
            {
                return x.CompareTo(y);
            }
            catch(RuntimeException)
            {
                // Сравнение типов не поддерживается
                return x.AsString().CompareTo(y.AsString());
            }
        }

        [ContextMethod("СортироватьПоПредставлению", "SortByPresentation")]
        public void SortByPresentation(SortDirectionEnum? direction = null)
        {
            if (direction == null || direction == SortDirectionEnum.Asc)
            {
                _items.Sort((x, y) => x.Presentation.CompareTo(y.Presentation));
            }
            else
            {
                _items.Sort((x, y) => y.Presentation.CompareTo(x.Presentation));
            }
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue item)
        {
            int indexSource = IndexByValue(item);

            _items.RemoveAt(indexSource);
        }

        #region Collection Context

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _items.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public IEnumerator<ValueListItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        [ScriptConstructor]
        public static ValueListImpl Constructor()
        {
            return new ValueListImpl();
        }

    }
}
