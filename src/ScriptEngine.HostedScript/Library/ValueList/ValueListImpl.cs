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

        [ContextMethod("Сдвинуть", "Move")]
        public void Move(IValue item, int direction)
        {
            ValueListItem itemObject;
            if(item.DataType == Machine.DataType.Number)
            {
                itemObject = GetValue(item);
            }
            else
            {
                var tmpObject = item.GetRawValue().AsObject() as ValueListItem;
                if (tmpObject == null)
                    throw RuntimeException.InvalidArgumentType();

                itemObject = tmpObject;
            }

            var index_source = this.IndexOf(itemObject);
            if (index_source < 0)
                throw new RuntimeException("Элемент не принадлежит списку значений");

            int index_dest = (index_source + direction) % _items.Count();
            while (index_dest < 0)
                index_dest += _items.Count();

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
            // TODO: надо проверить все это дело
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
        public void SortByValue(SelfAwareEnumValue<SortDirectionEnum> direction)
        {
            var enumInstance = GlobalsManager.GetEnum<SortDirectionEnum>();
            if(direction == enumInstance.Asc)
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
        public void SortByPresentation(SelfAwareEnumValue<SortDirectionEnum> direction)
        {
            var enumInstance = GlobalsManager.GetEnum<SortDirectionEnum>();
            if (direction == enumInstance.Asc)
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
            ValueListItem itemObject;
            if (item.DataType == Machine.DataType.Number)
            {
                itemObject = GetValue(item);
            }
            else
            {
                var tmpObject = item.GetRawValue().AsObject() as ValueListItem;
                if (tmpObject == null)
                    throw RuntimeException.InvalidArgumentType();

                itemObject = tmpObject;
            }

            var indexSource = IndexOf(itemObject);
            if (indexSource < 0)
                throw new RuntimeException("Элемент не принадлежит списку значений");

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
