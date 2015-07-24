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

namespace ScriptEngine.HostedScript.Library.ValueTable
{
    [ContextClass("ТаблицаЗначений", "ValueTable")]
    class ValueTable : AutoContext<ValueTable>, ICollectionContext
    {
        private ValueTableColumnCollection _columns = new ValueTableColumnCollection();
        private List<ValueTableRow> _rows = new List<ValueTableRow>();
        private CollectionIndexes _indexes = new CollectionIndexes();

        public ValueTable()
        {
        }

        [ContextProperty("Колонки", "Columns")]
        public ValueTableColumnCollection Columns
        {
            get { return _columns; }
        }

        [ContextProperty("Индексы", "Indexes")]
        public CollectionIndexes Indexes
        {
            get { return _indexes; }
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
        public void Delete(IValue Row)
        {
            Row = Row.GetRawValue();
            int index;
            if (Row is ValueTableRow)
            {
                index = _rows.IndexOf(Row as ValueTableRow);
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
            ValueTableColumn Column = Columns.GetColumnByIIndex(ColumnIndex);
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

            foreach (ValueTableRow row in _rows)
            {
                result.Add(row.Get(Column));
            }

            return result;
        }

        private List<ValueTableColumn> GetProcessingColumnList(string ColumnNames)
        {
            List<ValueTableColumn> processing_list = new List<ValueTableColumn>();
            if (ColumnNames != null)
            {

                if (ColumnNames.Trim().Length == 0)
                {
                    // Передали пустую строку вместо списка колонок
                    return processing_list;
                }

                string[] column_names = ColumnNames.Split(',');
                foreach (string name in column_names)
                {
                    ValueTableColumn Column = Columns.FindColumnByName(name.Trim());

                    if (Column == null)
                        throw RuntimeException.PropNotFoundException(name.Trim());

                    processing_list.Add(Column);
                }
            }
            else
            {
                foreach (ValueTableColumn Column in _columns)
                    processing_list.Add(Column);
            }
            return processing_list;
        }

        [ContextMethod("ЗаполнитьЗначения", "FillValues")]
        public void FillValues(IValue Value, string ColumnNames = null)
        {
            List<ValueTableColumn> processing_list = GetProcessingColumnList(ColumnNames);
            foreach (ValueTableRow row in _rows)
            {
                foreach (ValueTableColumn col in processing_list)
                {
                    row.Set(col, Value);
                }
            }
        }

        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(IValue Row)
        {
            Row = Row.GetRawValue();

            if (Row is ValueTableRow)
                return _rows.IndexOf(Row as ValueTableRow);

            return -1;
        }

        [ContextMethod("Итог", "Total")]
        public IValue Total(IValue ColumnIndex)
        {
            ValueTableColumn Column = Columns.GetColumnByIIndex(ColumnIndex);
            bool has_data = false;
            decimal Result = 0;

            foreach (ValueTableRow row in _rows)
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
            List<ValueTableColumn> processing_list = GetProcessingColumnList(ColumnNames);
            foreach (ValueTableRow row in _rows)
            {
                foreach (ValueTableColumn col in processing_list)
                {
                    IValue current = row.Get(col);
                    if (Value.Equals(current))
                        return row;
                }
            }
            return ValueFactory.Create();
        }

        private bool CheckFilterCriteria(ValueTableRow Row, StructureImpl Filter)
        {
            foreach (KeyAndValueImpl kv in Filter)
            {
                ValueTableColumn Column = Columns.FindColumnByName(kv.Key.AsString());
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

            foreach (ValueTableRow row in _rows)
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
        public ValueTableRow Get(int index)
        {
            if (index < 0 || index >= Count())
                throw RuntimeException.InvalidArgumentValue();
            return _rows[index];
        }

        [ContextMethod("Свернуть", "GroupBy")]
        public void GroupBy(string GroupColumnNames, string AggregateColumnNames = null)
        {

            // TODO: Сворачиваем за N^2. Переделать на N*log(N)

            List<ValueTableColumn> GroupColumns = GetProcessingColumnList(GroupColumnNames);
            List<ValueTableColumn> AggregateColumns = GetProcessingColumnList(AggregateColumnNames);

            List<ValueTableRow> new_rows = new List<ValueTableRow>();

            foreach (ValueTableRow row in _rows)
            {

                StructureImpl search = new StructureImpl();

                foreach (ValueTableColumn Column in GroupColumns)
                    search.Insert(Column.Name, row.Get(Column));

                ValueTableRow new_row = null;

                foreach (ValueTableRow nrow in new_rows)
                {
                    if (CheckFilterCriteria(nrow, search))
                    {
                        new_row = nrow;
                        break;
                    }
                }

                if (new_row == null)
                {
                    new_row = new ValueTableRow(this);
                    foreach (ValueTableColumn Column in GroupColumns)
                        new_row.Set(Column, row.Get(Column));

                    new_rows.Add(new_row);
                }

                foreach (ValueTableColumn Column in AggregateColumns)
                {
                    IValue old = new_row.Get(Column);
                    decimal d_old;

                    if (old.DataType != Machine.DataType.Number)
                        d_old = 0;
                    else
                        d_old = old.AsNumber();

                    IValue current = row.Get(Column);
                    decimal d_current;

                    if (current.DataType != Machine.DataType.Number)
                        d_current = 0;
                    else
                        d_current = current.AsNumber();

                    new_row.Set(Column, ValueFactory.Create(d_old + d_current));
                }

            }

            _rows.Clear();
            _rows.AddRange(new_rows);

            {
                int i = 0;
                while (i < _columns.Count())
                {
                    ValueTableColumn Column = _columns.FindColumnByIndex(i);
                    if (GroupColumns.IndexOf(Column) == -1 && AggregateColumns.IndexOf(Column) == -1)
                        _columns.Delete(Column);
                    else
                        ++i;
                }
            }
        }
        
        [ContextMethod("Сдвинуть", "Move")]
        public void Move(IValue Row, int Offset)
        {
            Row = Row.GetRawValue();

            int index_source;
            if (Row is ValueTableRow)
                index_source = _rows.IndexOf(Row as ValueTableRow);
            else if (Row.DataType == Machine.DataType.Number)
                index_source = decimal.ToInt32(Row.AsNumber());
            else
                throw RuntimeException.InvalidArgumentType();

            if (index_source < 0 || index_source >= _rows.Count())
                throw RuntimeException.InvalidArgumentValue();

            int index_dest = (index_source + Offset) % _rows.Count();
            while (index_dest < 0)
                index_dest += _rows.Count();

            ValueTableRow tmp = _rows[index_source];

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
        public ValueTable CopyColumns(string ColumnNames = null)
        {
            ValueTable Result = new ValueTable();

            List<ValueTableColumn> columns = GetProcessingColumnList(ColumnNames);

            foreach (ValueTableColumn Column in columns)
            {
                Result.Columns.Add(Column.Name, Column.ValueType, Column.Title); // TODO: Доработать после увеличения предела количества параметров
            }

            return Result;
        }

        [ContextMethod("Скопировать", "Copy")]
        public ValueTable Copy(IValue Rows = null, string ColumnNames = null)
        {
            ValueTable Result = CopyColumns(ColumnNames);
            List<ValueTableColumn> columns = GetProcessingColumnList(ColumnNames);
            
            IEnumerable<ValueTableRow> requestedRows;
            if (Rows == null)
            {
                requestedRows = _rows;
            }
            else
            {
                var rowsArray = Rows.GetRawValue() as ArrayImpl;
                if(rowsArray == null)
                    throw RuntimeException.InvalidArgumentType();

                requestedRows = rowsArray.Select(x =>
                {
                    var vtr = x.GetRawValue() as ValueTableRow;
                    if (vtr == null || vtr.Owner() != this)
                        throw RuntimeException.InvalidArgumentValue();
                    
                    return vtr;
                });
            }

            var columnMap = new Dictionary<ValueTableColumn, ValueTableColumn>();
            foreach (var column in columns)
            {
                var destinationColumn = Result.Columns.FindColumnByName(column.Name);
                columnMap.Add(column, destinationColumn);
            }

            foreach (var row in requestedRows)
            {
                ValueTableRow new_row = Result.Add();
                foreach (ValueTableColumn Column in columns)
                {
                    new_row.Set(columnMap[Column], row.Get(Column));
                }
            }

            return Result;
        }

        private struct ValueTableSortRule
        {
            public ValueTableColumn Column;
            public int direction; // 1 = asc, -1 = desc
        }

        private List<ValueTableSortRule> GetSortRules(string Columns)
        {

            string[] a_columns = Columns.Split(',');

            List<ValueTableSortRule> Rules = new List<ValueTableSortRule>();

            foreach (string column in a_columns)
            {
                string[] description = column.Trim().Split(' ');
                if (description.Count() == 0)
                    throw RuntimeException.PropNotFoundException(""); // TODO: WrongColumnNameException

                ValueTableSortRule Desc = new ValueTableSortRule();
                Desc.Column = this.Columns.FindColumnByName(description[0]);

                if (description.Count() > 1)
                {
                    if (String.Compare(description[1], "DESC", true) == 0 || String.Compare(description[1], "УБЫВ", true) == 0)
                        Desc.direction = -1;
                }
                else
                    Desc.direction = 1;

                Rules.Add(Desc);
            }

            return Rules;
        }

        private class RowComparator : IComparer<ValueTableRow>
        {
            List<ValueTableSortRule> Rules;
            GenericIValueComparer _comparer = new GenericIValueComparer();

            public RowComparator(List<ValueTableSortRule> Rules)
            {
                if (Rules.Count() == 0)
                    throw RuntimeException.InvalidArgumentValue();

                this.Rules = Rules;
            }

            private int OneCompare(ValueTableRow x, ValueTableRow y, ValueTableSortRule Rule)
            {
                IValue xValue = x.Get(Rule.Column);
                IValue yValue = y.Get(Rule.Column);

                int result = _comparer.Compare(xValue, yValue) * Rule.direction;

                return result;
            }

            public int Compare(ValueTableRow x, ValueTableRow y)
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
        public void Sort(string columns, IValue Comparator = null)
        {
            _rows.Sort(new RowComparator(GetSortRules(columns)));
        }

        [ContextMethod("ВыбратьСтроку", "ChooseRow")]
        public void ChooseRow(string Title = null, IValue StartRow = null)
        {
            throw new NotSupportedException();
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

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new ValueTable();
        }
    }
}
