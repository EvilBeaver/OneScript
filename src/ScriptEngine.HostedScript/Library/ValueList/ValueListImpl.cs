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
    public class ValueListImpl : AutoContext<ValueListImpl>, ICollectionContext
    {
        List<ValueListItem> _items = new List<ValueListItem>();

        [ContextMethod("Добавить", "Add")]
        public ValueListItem Add(IValue value, string presentation = null, bool check = false, IValue picture = null)
        {
            var newIndex = _items.Count;
            var newItem = new ValueListItem();
            newItem.Value = value;
            newItem.Presentation = presentation;
            newItem.Check = check;
            newItem.Picture = picture;

            _items.Add(newItem);
            return newItem;
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
        public IValue GetValue(IValue index)
        {
            int numericIndex = (int)index.AsNumber();
            return _items[numericIndex];
        }

        #region Collection Context

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _items.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(_items.GetEnumerator());
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            return GetManagedIterator();
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
