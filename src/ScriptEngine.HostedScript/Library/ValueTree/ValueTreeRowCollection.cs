/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.ValueTree
{
    [ContextClass("КоллекцияСтрокДереваЗначений", "ValueTreeRowCollection")]
    class ValueTreeRowCollection : AutoContext<ValueTreeRowCollection>, ICollectionContext
    {

        private List<ValueTreeRow> _rows = new List<ValueTreeRow>();
        private ValueTreeRow _parent;
        private ValueTree _owner;
        private int _level;

        public ValueTreeRowCollection(ValueTree owner, ValueTreeRow parent, int level)
        {
            _owner = owner;
            _parent = parent;
            _level = level;
        }

        [ContextProperty("Родитель", "Parent")]
        public IValue Parent
        {
            get
            {
                if (_parent != null)
                    return _parent;
                return ValueFactory.Create();
            }
        }

        private ValueTreeColumnCollection Columns
        {
            get
            {
                return _owner.Columns;
            }
        }

        [ContextMethod("Владелец", "Owner")]
        public ValueTree Owner()
        {
            return _owner;
        }

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _rows.Count();
        }

        [ContextMethod("Добавить", "Add")]
        public ValueTreeRow Add()
        {
            ValueTreeRow row = new ValueTreeRow(Owner(), _parent, _level);
            _rows.Add(row);
            return row;
        }

        [ContextMethod("Вставить", "Insert")]
        public ValueTreeRow Insert(int index)
        {
            ValueTreeRow row = new ValueTreeRow(Owner(), _parent, _level);
            _rows.Insert(index, row);
            return row;
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue Row)
        {
            Row = Row.GetRawValue();
            int index;
            if (Row is ValueTreeRow)
            {
                index = _rows.IndexOf(Row as ValueTreeRow);
                if (index == -1)
                    throw RuntimeException.InvalidArgumentValue();
            }
            else
            {
                index = Decimal.ToInt32(Row.AsNumber());
            }
            _rows.RemoveAt(index);
        }

        [ContextMethod("ЗагрузитьКолонку", "LoadColumn")]
        public void LoadColumn(IValue Values, IValue ColumnIndex)
        {
            ValueTreeColumn Column = Columns.GetColumnByIIndex(ColumnIndex);
            var row_iterator = _rows.GetEnumerator();
            var array_iterator = (Values as ArrayImpl).GetEnumerator();

            while (row_iterator.MoveNext() && array_iterator.MoveNext())
            {
                row_iterator.Current.Set(ColumnIndex, array_iterator.Current);
            }
        }

        [ContextMethod("ВыгрузитьКолонку", "UnloadColumn")]
        public ArrayImpl UnloadColumn(IValue Column)
        {
            ArrayImpl result = new ArrayImpl();

            foreach (ValueTreeRow row in _rows)
            {
                result.Add(row.Get(Column));
            }

            return result;
        }



        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(IValue Row)
        {
            Row = Row.GetRawValue();

            if (Row is ValueTreeRow)
                return _rows.IndexOf(Row as ValueTreeRow);

            return -1;
        }

        [ContextMethod("Итог", "Total")]
        public IValue Total(IValue ColumnIndex)
        {
            ValueTreeColumn Column = Columns.GetColumnByIIndex(ColumnIndex);
            bool has_data = false;
            decimal Result = 0;

            foreach (ValueTreeRow row in _rows)
            {
                IValue current_value = row.Get(Column);
                if (current_value.DataType == Machine.DataType.Number)
                {
                    has_data = true;
                    Result += current_value.AsNumber();
                }
            }

            if (has_data)
                return ValueFactory.Create(Result);

            return ValueFactory.Create();
        }

        [ContextMethod("Найти", "Find")]
        public IValue Find(IValue Value, string ColumnNames = null)
        {
            List<ValueTreeColumn> processing_list = Columns.GetProcessingColumnList(ColumnNames);
            foreach (ValueTreeRow row in _rows)
            {
                foreach (ValueTreeColumn col in processing_list)
                {
                    IValue current = row.Get(col);
                    if (Value.Equals(current))
                        return row;
                }
            }
            return ValueFactory.Create();
        }

        private bool CheckFilterCriteria(ValueTreeRow Row, StructureImpl Filter)
        {
            foreach (KeyAndValueImpl kv in Filter)
            {
                ValueTreeColumn Column = Columns.FindColumnByName(kv.Key.AsString());
                if (Column == null)
                    throw RuntimeException.PropNotFoundException(kv.Key.AsString());

                IValue current = Row.Get(Column);
                if (!current.Equals(kv.Value))
                    return false;
            }
            return true;
        }

        [ContextMethod("НайтиСтроки", "FindRows")]
        public ArrayImpl FindRows(IValue Filter)
        {
            if (!(Filter is StructureImpl))
                throw RuntimeException.InvalidArgumentType();

            ArrayImpl Result = new ArrayImpl();

            foreach (ValueTreeRow row in _rows)
            {
                if (CheckFilterCriteria(row, Filter as StructureImpl))
                    Result.Add(row);
            }

            return Result;
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _rows.Clear();
        }

        [ContextMethod("Получить", "Get")]
        public ValueTreeRow Get(int index)
        {
            if (index < 0 || index >= Count())
                throw RuntimeException.InvalidArgumentValue();
            return _rows[index];
        }

        [ContextMethod("Сдвинуть", "Move")]
        public void Move(IValue Row, int Offset)
        {
            Row = Row.GetRawValue();

            int index_source;
            if (Row is ValueTreeRow)
                index_source = _rows.IndexOf(Row as ValueTreeRow);
            else if (Row.DataType == Machine.DataType.Number)
                index_source = decimal.ToInt32(Row.AsNumber());
            else
                throw RuntimeException.InvalidArgumentType();

            if (index_source < 0 || index_source >= _rows.Count())
                throw RuntimeException.InvalidArgumentValue();

            int index_dest = (index_source + Offset) % _rows.Count();
            while (index_dest < 0)
                index_dest += _rows.Count();

            ValueTreeRow tmp = _rows[index_source];

            if (index_source < index_dest)
            {
                _rows.Insert(index_dest + 1, tmp);
                _rows.RemoveAt(index_source);
            }
            else
            {
                _rows.RemoveAt(index_source);
                _rows.Insert(index_dest, tmp);
            }

        }

        [ContextMethod("СкопироватьКолонки", "CopyColumns")]
        public ValueTree CopyColumns(string ColumnNames = null)
        {
            ValueTree Result = new ValueTree();

            List<ValueTreeColumn> columns = Columns.GetProcessingColumnList(ColumnNames);

            foreach (ValueTreeColumn Column in columns)
            {
                Result.Columns.Add(Column.Name, Column.ValueType, Column.Title, Column.Width);
            }

            return Result;
        }

        private struct ValueTreeSortRule
        {
            public ValueTreeColumn Column;
            public int direction; // 1 = asc, -1 = desc
        }

        private List<ValueTreeSortRule> GetSortRules(string Columns)
        {

            string[] a_columns = Columns.Split(',');

            List<ValueTreeSortRule> Rules = new List<ValueTreeSortRule>();

            foreach (string column in a_columns)
            {
                string[] description = column.Trim().Split(' ');
                if (description.Count() == 0)
                    throw RuntimeException.PropNotFoundException(""); // TODO: WrongColumnNameException

                ValueTreeSortRule Desc = new ValueTreeSortRule();
                Desc.Column = this.Columns.FindColumnByName(description[0]);
                if (Desc.Column == null)
                    throw RuntimeException.PropNotFoundException(description[0]);

                if (description.Count() > 1)
                {
                    if (String.Compare(description[1], "DESC", true) == 0 || String.Compare(description[1], "УБЫВ", true) == 0)
                        Desc.direction = -1;
                    else
                        Desc.direction = 1;
                }
                else
                    Desc.direction = 1;

                Rules.Add(Desc);
            }

            return Rules;
        }

        private class RowComparator : IComparer<ValueTreeRow>
        {
            List<ValueTreeSortRule> Rules;
            GenericIValueComparer _comparer = new GenericIValueComparer();

            public RowComparator(List<ValueTreeSortRule> Rules)
            {
                if (Rules.Count() == 0)
                    throw RuntimeException.InvalidArgumentValue();

                this.Rules = Rules;
            }

            private int OneCompare(ValueTreeRow x, ValueTreeRow y, ValueTreeSortRule Rule)
            {
                IValue xValue = x.Get(Rule.Column);
                IValue yValue = y.Get(Rule.Column);

                int result = _comparer.Compare(xValue, yValue) * Rule.direction;

                return result;
            }

            public int Compare(ValueTreeRow x, ValueTreeRow y)
            {
                int i = 0, r;
                while ((r = OneCompare(x, y, Rules[i])) == 0)
                {
                    if (++i >= Rules.Count())
                        return 0;
                }

                return r;
            }
        }

        [ContextMethod("Сортировать", "Sort")]
        public void Sort(string columns, bool SortChildren, IValue Comparator = null)
        {
            Sort(new RowComparator(GetSortRules(columns)), SortChildren);
        }

        private void Sort(RowComparator Comparator, bool SortChildren)
        {
            _rows.Sort(Comparator);

            if (SortChildren)
            {
                foreach (var row in _rows)
                {
                    row.Rows.Sort(Comparator, SortChildren);
                }
            }
        }

        [ContextMethod("ВыбратьСтроку", "ChooseRow")]
        public void ChooseRow(string Title = null, IValue StartRow = null)
        {
            throw new NotSupportedException();
        }

        internal void CopyFrom(ValueTreeRowCollection src)
        {
            _rows.Clear();
            ValueTreeColumnCollection Columns = Owner().Columns;

            foreach (ValueTreeRow row in src._rows)
            {
                ValueTreeRow new_row = Add();
                foreach (ValueTreeColumn Column in Columns)
                {
                    new_row.Set(Column, row.Get(ValueFactory.Create(Column.Name)));
                }
                new_row.Rows.CopyFrom(row.Rows);
            }
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

        public override IValue GetIndexedValue(IValue index)
        {
            return Get((int)index.AsNumber());
        }
    }
}
