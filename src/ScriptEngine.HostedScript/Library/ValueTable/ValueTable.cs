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
    /// <summary>
    /// Объект для работы с данными в табличном виде. 
    /// Представляет из себя коллекцию строк с заранее заданной структурой.
    /// </summary>
    [ContextClass("ТаблицаЗначений", "ValueTable")]
    public class ValueTable : AutoContext<ValueTable>, ICollectionContext, IEnumerable<ValueTableRow>
    {
        private readonly ValueTableColumnCollection _columns;
        private readonly List<ValueTableRow> _rows;
        private readonly CollectionIndexes _indexes;

        public ValueTable()
        {
            _columns = new ValueTableColumnCollection(this);
            _rows = new List<ValueTableRow>();
            _indexes = new CollectionIndexes();
    }

        /// <summary>
        /// Коллекция колонок
        /// </summary>
        /// <value>КоллекцияКолонокТаблицыЗначений</value>
        [ContextProperty("Колонки", "Columns")]
        public ValueTableColumnCollection Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Коллекция индексов
        /// </summary>
        /// <value>ИндексыКоллекции</value>
        [ContextProperty("Индексы", "Indexes")]
        public CollectionIndexes Indexes
        {
            get { return _indexes; }
        }

        /// <summary>
        /// Количество строк в Таблице значений
        /// </summary>
        /// <returns>Число</returns>
        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _rows.Count;
        }

        /// <summary>
        /// Добавляет строку в конец Таблицы значений
        /// </summary>
        /// <returns>СтрокаТаблицыЗначений</returns>
        [ContextMethod("Добавить", "Add")]
        public ValueTableRow Add()
        {
            ValueTableRow row = new ValueTableRow(this);
            _rows.Add(row);
            return row;
        }

        /// <summary>
        /// Вставляет строку в указанную позицию
        /// </summary>
        /// <param name="index">Число - Индекс позиции куда будет произведена вставка</param>
        /// <returns>СтрокаТаблицыЗначений</returns>
        [ContextMethod("Вставить", "Insert")]
        public ValueTableRow Insert(int index)
        {
            ValueTableRow row = new ValueTableRow(this);
            _rows.Insert(index, row);
            return row;
        }

        /// <summary>
        /// Удаляет строку
        /// </summary>
        /// <param name="row">
        /// СтрокаТаблицыЗначений - Удаляемая строка
        /// Число - Индекс удаляемой строки
        /// </param>
        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue row)
        {
            int index = IndexByValue(row);

            _rows.RemoveAt(index);
        }

        /// <summary>
        /// Загружает значения в колонку
        /// </summary>
        /// <param name="values">Массив - Значения для загрузки в колонку</param>
        /// <param name="columnIndex">
        /// Строка - Имя колонки для загрузки
        /// Число - Индекс колонки для загрузки
        /// КолонкаТаблицыЗначений - Колонка для загрузки
        /// </param>
        [ContextMethod("ЗагрузитьКолонку", "LoadColumn")]
        public void LoadColumn(IValue values, IValue columnIndex)
        {
            // ValueTableColumn Column = Columns.GetColumnByIIndex(ColumnIndex);
            var row_iterator = _rows.GetEnumerator();
            var array_iterator = (values as ArrayImpl).GetEnumerator();

            while (row_iterator.MoveNext() && array_iterator.MoveNext())
            {
                row_iterator.Current.Set(columnIndex, array_iterator.Current);
            }
        }

        /// <summary>
        /// Выгружает значения колонки в новый массив
        /// </summary>
        /// <param name="column">
        /// Строка - Имя колонки для выгрузки
        /// Число - Индекс колонки для выгрузки
        /// КолонкаТаблицыЗначений - Колонка для выгрузки
        /// </param>
        /// <returns>Массив</returns>
        [ContextMethod("ВыгрузитьКолонку", "UnloadColumn")]
        public ArrayImpl UnloadColumn(IValue column)
        {
            ArrayImpl result = new ArrayImpl();

            foreach (ValueTableRow row in _rows)
            {
                result.Add(row.Get(column));
            }

            return result;
        }

        private List<ValueTableColumn> GetProcessingColumnList(string ColumnNames, bool EmptyListInCaseOfNull = false)
        {
            List<ValueTableColumn> processing_list = new List<ValueTableColumn>();
            if (ColumnNames != null)
            {
                if (ColumnNames.Trim().Length == 0)
                {
                    // Передали пустую строку вместо списка колонок
                    return processing_list;
                }

                foreach (string column_name in ColumnNames.Split(','))
                {
                    string name = column_name.Trim();
                    ValueTableColumn Column = Columns.FindColumnByName(name);

                    if (Column == null)
                        throw WrongColumnNameException(name);

                    if (processing_list.Find( x=> x.Name==name ) == null)
                        processing_list.Add(Column);
                }
            }
            else if (!EmptyListInCaseOfNull)
            {
                foreach (ValueTableColumn Column in _columns)
                    processing_list.Add(Column);
            }
            return processing_list;
        }

        /// <summary>
        /// Заполнить колонку/колонки указанным значением
        /// </summary>
        /// <param name="value">Произвольный - Устанавливаемое значение</param>
        /// <param name="columnNames">Строка - Список имен колонок для установки значения (разделены запятыми)</param>
        [ContextMethod("ЗаполнитьЗначения", "FillValues")]
        public void FillValues(IValue value, string columnNames = null)
        {
            List<ValueTableColumn> processing_list = GetProcessingColumnList(columnNames);
            foreach (ValueTableRow row in _rows)
            {
                foreach (ValueTableColumn col in processing_list)
                {
                    row.Set(col, value);
                }
            }
        }

        /// <summary>
        /// Получить индекс указанной строки
        /// </summary>
        /// <param name="row">СтрокаТаблицыЗначений - Строка таблицы значений, для которой необходимо определить индекс</param>
        /// <returns>Число - Индекс в коллекции, если не найдено возвращает -1</returns>
        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(IValue row)
        {
            row = row.GetRawValue();

            if (row is ValueTableRow)
                return _rows.IndexOf(row as ValueTableRow);

            return -1;
        }

        /// <summary>
        /// Сумма значений всех строк указанной колонки
        /// </summary>
        /// <param name="columnIndex">
        /// Строка - Имя колонки для суммирования
        /// Число - Индекс колонки для суммирования
        /// КолонкаТаблицыЗначений - Колонка для суммирования
        /// </param>
        /// <returns>Число</returns>
        [ContextMethod("Итог", "Total")]
        public IValue Total(IValue columnIndex)
        {
            ValueTableColumn Column = Columns.GetColumnByIIndex(columnIndex);
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

        /// <summary>
        /// Осуществляет поиск значения в указанных колонках
        /// </summary>
        /// <param name="value">Произвольный - Искомое значение</param>
        /// <param name="columnNames">Строка - Список имен колонок для поиска значения (разделены запятыми). 
        /// Если параметр не указан - ищет по всем колонкам. По умолчанию: пустая строка</param>
        /// <returns>СтрокаТаблицыЗначений - если строка найдена, иначе Неопределено</returns>
        [ContextMethod("Найти", "Find")]
        public IValue Find(IValue value, string columnNames = null)
        {
            List<ValueTableColumn> processing_list = GetProcessingColumnList(columnNames);
            foreach (ValueTableRow row in _rows)
            {
                foreach (ValueTableColumn col in processing_list)
                {
                    IValue current = row.Get(col);
                    if (value.Equals(current))
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
                    throw WrongColumnNameException(kv.Key.AsString());

                IValue current = Row.Get(Column);
                if (!current.Equals(kv.Value))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Поиск строк по условию
        /// </summary>
        /// <param name="filter">Структура - Условия поиска. Ключ - имя колонки, значение - искомое значение</param>
        /// <returns>Массив - Массив ссылок на строки, удовлетворяющих условию поиска</returns>
        [ContextMethod("НайтиСтроки", "FindRows")]
        public ArrayImpl FindRows(IValue filter)
        {
            var filterStruct = filter.GetRawValue() as StructureImpl;

            if (filterStruct == null)
                throw RuntimeException.InvalidArgumentType();

            ArrayImpl Result = new ArrayImpl();

            foreach (ValueTableRow row in _rows)
            {
                if (CheckFilterCriteria(row, filterStruct))
                    Result.Add(row);
            }

            return Result;
        }

        /// <summary>
        /// Удаляет все строки. Структура колонок не меняется.
        /// </summary>
        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _rows.Clear();
        }

        /// <summary>
        /// Получить строку по индексу
        /// </summary>
        /// <param name="index">Число - Индекс строки</param>
        /// <returns>СтрокаТаблицыЗначений</returns>
        [ContextMethod("Получить", "Get")]
        public ValueTableRow Get(int index)
        {
            if (index < 0 || index >= Count())
                throw RuntimeException.InvalidArgumentValue();
            return _rows[index];
        }

        /// <summary>
        /// Сворачиваются (группируются) строки по указанным колонкам измерениям, суммируются колонки ресурсов. 
        /// Колонки не указанные ни в измерениях ни в ресурсах удаляются.
        /// </summary>
        /// <param name="groupColumnNames">Строка - Имена колонок для сворачивания (изменения), разделены запятыми</param>
        /// <param name="aggregateColumnNames">Строка - Имена колонок для суммирования (ресурсы), разделены запятыми</param>
        [ContextMethod("Свернуть", "GroupBy")]
        public void GroupBy(string groupColumnNames, string aggregateColumnNames = null)
        {
            List<ValueTableColumn> GroupColumns = GetProcessingColumnList(groupColumnNames, true);
            List<ValueTableColumn> AggregateColumns = GetProcessingColumnList(aggregateColumnNames, true);

            foreach (ValueTableColumn group_column in GroupColumns )
                if ( AggregateColumns.Find(x => x.Name==group_column.Name)!=null )
                    throw ColumnsMixedException(group_column.Name);

            var uniqueRows = new Dictionary<ValueTableRow, ValueTableRow>(new RowsByColumnsEqComparer(GroupColumns) );
            int new_idx = 0;

            foreach (ValueTableRow row in _rows)
            {
                if (uniqueRows.ContainsKey(row))
                {
                    ValueTableRow old_row = uniqueRows[row];

                    foreach (var Column in AggregateColumns)
                    {
                        IValue current = row.Get(Column);
                        if (current.DataType == DataType.Number)
                        {
                            decimal sum = old_row.Get(Column).AsNumber() + current.AsNumber();
                            old_row.Set(Column, ValueFactory.Create(sum));
                        }
                    }
                }
                else
                {
                    uniqueRows.Add(row, row);

                    ValueTableRow new_row = _rows[new_idx++];

                    foreach (var Column in GroupColumns)
                        new_row.Set(Column, row.Get(Column));

                    foreach (var Column in AggregateColumns)
                        if (new_row.Get(Column).DataType != DataType.Number)
                            new_row.Set(Column, ValueFactory.Create(0));
                }
            }

            _rows.RemoveRange(new_idx, _rows.Count()-new_idx);

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

        private class RowsByColumnsEqComparer : IEqualityComparer<ValueTableRow>
        {
            private List<ValueTableColumn> _columns;

            public RowsByColumnsEqComparer(List<ValueTableColumn> columns)
            {
                _columns = columns;
            }

            public bool Equals(ValueTableRow row1, ValueTableRow row2)
            {
                foreach (var column in _columns)
                {
                    if (!row1.Get(column).Equals(row2.Get(column)))
                        return false;
                }
                return true;
            }

            public int GetHashCode(ValueTableRow row)
            {
                int hash = 0;
                foreach (var column in _columns)
                    hash ^= row.Get(column).AsString().GetHashCode();
                return hash;
            }
        }

        private int IndexByValue(IValue item)
        {
            item = item.GetRawValue();

            int index;

            if (item is ValueTableRow)
            {
                index = IndexOf(item as ValueTableRow);
                if (index == -1)
                    throw new RuntimeException("Строка не принадлежит таблице значений");
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

                if (index < 0 || index >= _rows.Count())
                    throw new RuntimeException("Значение индекса выходит за пределы диапазона");
            }

            return index;
        }

        /// <summary>
        /// Сдвигает строку на указанное количество позиций.
        /// </summary>
        /// <param name="row">
        /// СтрокаТаблицыЗначений - Строка которую сдвигаем
        /// Число - Индекс сдвигаемой строки
        /// </param>
        /// <param name="offset">Количество строк, на которое сдвигается строка. Если значение положительное - сдвиг вниз, иначе вверх</param>
        [ContextMethod("Сдвинуть", "Move")]
        public void Move(IValue row, int offset)
        {
            int index_source = IndexByValue(row);

            int index_dest = index_source + offset;

            if (index_dest < 0 || index_dest >= _rows.Count())
                throw RuntimeException.InvalidNthArgumentValue(2);

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

        /// <summary>
        /// Создает новую таблицу значений с указанными колонками. Данные не копируются.
        /// </summary>
        /// <param name="columnNames">Строка - Имена колонок для копирования, разделены запятыми</param>
        /// <returns>ТаблицаЗначений</returns>
        [ContextMethod("СкопироватьКолонки", "CopyColumns")]
        public ValueTable CopyColumns(string columnNames = null)
        {
            ValueTable Result = new ValueTable();

            List<ValueTableColumn> columns = GetProcessingColumnList(columnNames);

            foreach (ValueTableColumn Column in columns)
            {
                Result.Columns.Add(Column.Name, Column.ValueType, Column.Title, Column.Width);
            }

            return Result;
        }

        /// <summary>
        /// Создает новую таблицу значений с указанными строками и колонками. Если передан отбор - копирует строки удовлетворяющие отбору.
        /// Если не указаны строки - будут скопированы все строки. Если не указаны колонки - будут скопированы все колонки.
        /// Если не указаны оба параметра - будет создана полная копия таблицы значений.
        /// </summary>
        /// <param name="rows">
        /// Массив - Массив строк для отбора
        /// Структура - Параметры отбора. Ключ - Колонка, Значение - Значение отбора
        /// </param>
        /// <param name="columnNames">Строка - Имена колонок для копирования, разделены запятыми</param>
        /// <returns>ТаблицаЗначений</returns>
        [ContextMethod("Скопировать", "Copy")]
        public ValueTable Copy(IValue rows = null, string columnNames = null)
        {
            ValueTable Result = CopyColumns(columnNames);
            List<ValueTableColumn> columns = GetProcessingColumnList(columnNames);
            
            IEnumerable<ValueTableRow> requestedRows;
            if (rows == null)
            {
                requestedRows = _rows;
            }
            else
            {
                if (rows.SystemType.Equals(TypeManager.GetTypeByFrameworkType(typeof(StructureImpl))))
                    requestedRows = FindRows(rows).Select(x => x as ValueTableRow);
                else if (rows.SystemType.Equals(TypeManager.GetTypeByFrameworkType(typeof(ArrayImpl))))
                    requestedRows = GetRowsEnumByArray(rows);
                else
                    throw RuntimeException.InvalidArgumentType();
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

        private IEnumerable<ValueTableRow> GetRowsEnumByArray(IValue Rows)
        {
            IEnumerable<ValueTableRow> requestedRows;
            var rowsArray = Rows.GetRawValue() as ArrayImpl;
            if (rowsArray == null)
                throw RuntimeException.InvalidArgumentType();

            requestedRows = rowsArray.Select(x =>
            {
                var vtr = x.GetRawValue() as ValueTableRow;
                if (vtr == null || vtr.Owner() != this)
                    throw RuntimeException.InvalidArgumentValue();

                return vtr;
            });
            return requestedRows;
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
                    throw WrongColumnNameException();

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
            readonly List<ValueTableSortRule> Rules;

            readonly GenericIValueComparer _comparer = new GenericIValueComparer();

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

        /// <summary>
        /// Сортировать строки в таблице значений. Строки сортируются по порядку следования колонок для сортировки, с учетом варианта сортировки.
        /// </summary>
        /// <param name="columns">Строка - Имена колонок для сортировки. 
        /// После имени колонки, через пробел, можно указать направление сортировки: "Убыв" ("Desc") - по убыванию. Возр" ("Asc") - по возрастанию
        /// По умолчанию - по возрастанию.
        /// </param>
        /// <param name="comparator">СравнениеЗначений - правила сравнения значений при наличии различных типов данных в колонке.</param>
        [ContextMethod("Сортировать", "Sort")]
        public void Sort(string columns, IValue comparator = null)
        {
            _rows.Sort(new RowComparator(GetSortRules(columns)));
        }

        /// <summary>
        /// Не поддерживается
        /// </summary>
        /// <param name="title"></param>
        /// <param name="startRow"></param>
        [ContextMethod("ВыбратьСтроку", "ChooseRow")]
        public void ChooseRow(string title = null, IValue startRow = null)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<ValueTableRow> GetEnumerator()
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
        public static ValueTable Constructor()
        {
            return new ValueTable();
        }


        private static RuntimeException WrongColumnNameException()
        {
            return new RuntimeException("Неверное имя колонки");
        }

        private static RuntimeException WrongColumnNameException(string columnName)
        {
            return new RuntimeException(string.Format("Неверное имя колонки '{0}'", columnName));
        }

        private static RuntimeException ColumnsMixedException(string columnName)
        {
            return new RuntimeException(string.Format("Колонка '{0}' не может одновременно быть колонкой группировки и колонкой суммирования", columnName));
        }
    }
}
