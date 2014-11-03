using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Library;

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

        [ContextMethod("ЗагрузитьКолонку", "LoadColumn")]
        public void LoadColumn(IValue Values, IValue Column)
        {
            ValueTableColumn C = Columns.GetColumnByIIndex(Column);
            var row_iterator = _rows.GetEnumerator();
            var array_iterator = (Values as ArrayImpl).GetEnumerator();

            while (row_iterator.MoveNext() && array_iterator.MoveNext())
            {
                row_iterator.Current.Set(Column, array_iterator.Current);
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
                string[] column_names = ColumnNames.Split(',');
                foreach (string name in column_names)
                {
                    ValueTableColumn C = Columns.FindColumnByName(name.Trim());

                    if (C == null)
                        throw RuntimeException.PropNotFoundException(name.Trim());

                    processing_list.Add(C);
                }
            }
            else
            {
                foreach (ValueTableColumn C in _columns)
                    processing_list.Add(C);
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
        public int IndexOf(IValue row)
        {
            if (row is ValueTableRow)
                return _rows.IndexOf(row as ValueTableRow);

            return -1;
        }

        [ContextMethod("Итог", "Total")]
        public IValue Total(IValue Column)
        {
            ValueTableColumn C = Columns.GetColumnByIIndex(Column);
            bool has_data = false;
            decimal Result = 0;

            foreach (ValueTableRow row in _rows)
            {
                IValue current_value = row.Get(C);
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
                ValueTableColumn C = Columns.FindColumnByName(kv.Key.AsString());
                if (C == null)
                    throw RuntimeException.PropNotFoundException(kv.Key.AsString());

                IValue current = Row.Get(C);
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

            List<ValueTableColumn> group_columns = GetProcessingColumnList(GroupColumnNames);
            List<ValueTableColumn> agg_columns = GetProcessingColumnList(AggregateColumnNames);

            List<ValueTableRow> new_rows = new List<ValueTableRow>();

            foreach (ValueTableRow row in _rows)
            {

                StructureImpl search = new StructureImpl();

                foreach (ValueTableColumn C in group_columns)
                    search.Insert(C.Name, row.Get(C));

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
                    foreach (ValueTableColumn C in group_columns)
                        new_row.Set(C, row.Get(C));

                    new_rows.Add(new_row);
                }

                foreach (ValueTableColumn C in agg_columns)
                {
                    IValue old = new_row.Get(C);
                    decimal d_old;

                    if (old.DataType != Machine.DataType.Number)
                        d_old = 0;
                    else
                        d_old = old.AsNumber();

                    IValue current = row.Get(C);
                    decimal d_current;

                    if (current.DataType != Machine.DataType.Number)
                        d_current = 0;
                    else
                        d_current = current.AsNumber();

                    new_row.Set(C, ValueFactory.Create(d_old + d_current));
                }

            }

            _rows.Clear();
            _rows.AddRange(new_rows);

            // TODO: Убить старые колонки
        }
        
        [ContextMethod("Сдвинуть", "Move")]
        public void Move(IValue Row, int Offset)
        {
            int index_source;
            if (Row is ValueTableRow)
                index_source = _rows.IndexOf(Row as ValueTableRow);
            else if (Row.DataType == Machine.DataType.Number)
                index_source = decimal.ToInt32(Row.AsNumber());
            else
                throw RuntimeException.InvalidArgumentType();

            int index_dest = index_source + Offset;

            ValueTableRow tmp = _rows[index_dest];
            _rows[index_dest] = _rows[index_source];
            _rows[index_source] = tmp;
        }

        [ContextMethod("СкопироватьКолонки", "CopyColumns")]
        public ValueTable CopyColumns(string ColumnNames = null)
        {
            ValueTable Result = new ValueTable();

            List<ValueTableColumn> columns = GetProcessingColumnList(ColumnNames);

            foreach (ValueTableColumn C in columns)
            {
                Result.Columns.Add(C.Name, C.ValueType, C.Title); // TODO: Доработать после увеличения предела количества параметров
            }

            return Result;
        }

        [ContextMethod("Скопировать", "Copy")]
        public ValueTable Copy(IValue Rows = null, string ColumnNames = null)
        {
            ValueTable Result = CopyColumns(ColumnNames);
            List<ValueTableColumn> columns = GetProcessingColumnList(ColumnNames);

            // TODO: Переделать это убожество. Свернуть две ветки в одну, сделать соответствие объектов старых колонок и новых.

            if (Rows == null)
            {
                foreach (ValueTableRow row in _rows)
                {
                    ValueTableRow new_row = Result.Add();
                    foreach (ValueTableColumn C in columns)
                    {
                        new_row.Set(ValueFactory.Create(C.Name), row.Get(C));
                    }
                }
            }
            else
            {
                ArrayImpl a_Rows = Rows as ArrayImpl;
                foreach (IValue irow in a_Rows)
                {
                    ValueTableRow row = irow as ValueTableRow;

                    ValueTableRow new_row = Result.Add();
                    foreach (ValueTableColumn C in columns)
                    {
                        new_row.Set(ValueFactory.Create(C.Name), row.Get(C));
                    }
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

                int result = xValue.CompareTo(yValue) * Rule.direction;

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

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new ValueTable();
        }
    }
}
