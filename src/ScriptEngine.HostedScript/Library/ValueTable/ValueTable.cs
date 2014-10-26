using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.ValueTable
{
    [ContextClass("ТаблицаЗначений", "ValueTable")]
    class ValueTable : AutoContext<ValueTable>, ICollectionContext
    {
        private ValueTableColumnCollection _columns = new ValueTableColumnCollection();
        private List<ValueTableRow> _rows = new List<ValueTableRow>();

        public ValueTable()
        {
        }

        [ContextProperty("Колонки", "Columns")]
        public ValueTableColumnCollection Columns
        {
            get { return _columns; }
        }

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _rows.Count();
        }

        [ContextMethod("Добавить", "Add")]
        public ValueTableRow Add()
        {
            ValueTableRow row = new ValueTableRow(this);
            _rows.Add(row);
            return row;
        }

        [ContextMethod("ВставитЬ", "Insert")]
        public ValueTableRow Insert(int index)
        {
            ValueTableRow row = new ValueTableRow(this);
            _rows.Insert(index, row);
            return row;
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue row)
        {

            int index;
            if (row is ValueTableRow)
            {
                // TODO: Проверить индекс
                index = _rows.IndexOf(row as ValueTableRow);
            }
            else
            {
                // TODO: Переполнение int32
                index = Decimal.ToInt32(row.AsNumber());
            }
            _rows.RemoveAt(index);
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _rows)
            {
                yield return item;
            }
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new ValueTable();
        }
    }
}
