/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Commons;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.ValueTree
{
    /// <summary>
    /// Коллекция строк дерева значений.
    /// </summary>
    [ContextClass("КоллекцияСтрокДереваЗначений", "ValueTreeRowCollection", TypeUUID = "CEBF52F0-DA62-4058-9A22-0E659747E622")]
    public class ValueTreeRowCollection : AutoCollectionContext<ValueTreeRowCollection, ValueTreeRow>
    {
        private readonly List<ValueTreeRow> _rows = new List<ValueTreeRow>();
        private readonly ValueTreeRow _parent;
        private readonly ValueTree _owner;
        private readonly int _level;
        
        private static TypeDescriptor _instanceType = typeof(ValueTreeRowCollection).GetTypeFromClassMarkup();

        public ValueTreeRowCollection(ValueTree owner, ValueTreeRow parent, int level)
            : base(_instanceType)
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

        /// <summary>
        /// Возвращает дерево значений, в которе входит строка.
        /// </summary>
        /// <returns>ДеревоЗначений. Владелец строки.</returns>
        [ContextMethod("Владелец", "Owner")]
        public ValueTree Owner()
        {
            return _owner;
        }

        /// <summary>
        /// Возвращает количество строк.
        /// </summary>
        /// <returns>Число. Количество строк.</returns>
        [ContextMethod("Количество", "Count")]
        public override int Count()
        {
            return _rows.Count();
        }

        /// <summary>
        /// Добавляет строку в коллекцию.
        /// </summary>
        /// <returns>СтрокаДереваЗначений. Добавленная строка.</returns>
        [ContextMethod("Добавить", "Add")]
        public ValueTreeRow Add()
        {
            ValueTreeRow row = new ValueTreeRow(Owner(), _parent, _level);
            _rows.Add(row);
            return row;
        }

        /// <summary>
        /// Добавляет строку в коллекцию.
        /// </summary>
        /// <param name="index">Число. Индекс новой строки.</param>
        /// <returns>СтрокаДереваЗначений. Добавленная строка.</returns>
        [ContextMethod("Вставить", "Insert")]
        public ValueTreeRow Insert(int index)
        {
            ValueTreeRow row = new ValueTreeRow(Owner(), _parent, _level);
            _rows.Insert(index, row);
            return row;
        }

        /// <summary>
        /// Удаляет строку из коллекции.
        /// </summary>
        /// <param name="row">СтрокаДереваЗначений, Число. Удаляемая строка или её индекс.</param>
        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue row)
        {
            row = row.GetRawValue();
            int index;
            if (row is ValueTreeRow)
            {
                index = _rows.IndexOf(row as ValueTreeRow);
                if (index == -1)
                    throw RuntimeException.InvalidArgumentValue();
            }
            else
            {
                index = Decimal.ToInt32(row.AsNumber());
            }
            _rows.RemoveAt(index);
        }

        /// <summary>
        /// Загружает значения из массива в колонку.
        /// </summary>
        /// <param name="values">Массив. Значения.</param>
        /// <param name="columnIndex">КолонкаДереваЗначений, Число, Строка. Колонка, в которую будут загружены значения, её имя или индекс.</param>
        [ContextMethod("ЗагрузитьКолонку", "LoadColumn")]
        public void LoadColumn(IValue values, IValue columnIndex)
        {
            var rowIterator = _rows.GetEnumerator();
            var arrayIterator = (values as ArrayImpl).GetEnumerator();

            while (rowIterator.MoveNext() && arrayIterator.MoveNext())
            {
                rowIterator.Current.Set(columnIndex, arrayIterator.Current);
            }
        }

        /// <summary>
        /// Загружает значения из массива в колонку.
        /// </summary>
        /// <param name="column">КолонкаДереваЗначений, Число, Строка. Колонка, из которой будут выгружены значения, её имя или индекс.</param>
        /// <returns>Массив. Массив значений.</returns>
        [ContextMethod("ВыгрузитьКолонку", "UnloadColumn")]
        public ArrayImpl UnloadColumn(IValue column)
        {
            ArrayImpl result = new ArrayImpl();

            foreach (ValueTreeRow row in _rows)
            {
                result.Add(row.Get(column));
            }

            return result;
        }

        /// <summary>
        /// Определяет индекс строки.
        /// </summary>
        /// <param name="row">СтрокаДереваЗначений. Строка.</param>
        /// <returns>Число. Индекс строки в коллекции. Если строка не найдена, возвращается -1.</returns>
        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(IValue row)
        {
            row = row.GetRawValue();

            if (row is ValueTreeRow treeRow)
                return _rows.IndexOf(treeRow);

            return -1;
        }

        /// <summary>
        /// Суммирует значения в строках.
        /// </summary>
        /// <param name="columnIndex">КолонкаДереваЗначений, Строка, Число. Колонка, значения которой будут суммироваться.</param>
        /// <param name="includeChildren">Булево. Если Истина, в расчёт будут включены все вложенные строки.</param>
        /// <returns>Число. Вычисленная сумма.</returns>
        [ContextMethod("Итог", "Total")]
        public IValue Total(IValue columnIndex, bool includeChildren = false)
        {
            ValueTreeColumn column = Columns.GetColumnByIIndex(columnIndex);
            decimal result = 0;

            foreach (ValueTreeRow row in _rows)
            {
                IValue currentValue = row.Get(column);
                if (currentValue.SystemType == BasicTypes.Number)
                {
                    result += currentValue.AsNumber();
                }

                if (includeChildren)
                {
                    IValue childrenTotal = row.Rows.Total(columnIndex, includeChildren);
                    if (childrenTotal.SystemType == BasicTypes.Number)
                    {
                        result += childrenTotal.AsNumber();
                    }
                }
            }

            return ValueFactory.Create(result);
        }

        /// <summary>
        /// Ищет значение в строках дерева значений.
        /// </summary>
        /// <param name="value">Произвольный. Искомое значение.</param>
        /// <param name="columnNames">Строка. Список колонок через запятую, в которых будет производиться поиск. Необязательный параметр.</param>
        /// <param name="includeChildren">Булево. Если Истина, в поиск будут включены все вложенные строки. Необязательный параметр.</param>
        /// <returns>СтрокаДереваЗначений, Неопределено. Найденная строка или Неопределено, если строка не найдена.</returns>
        [ContextMethod("Найти", "Find")]
        public IValue Find(IValue value, string columnNames = null, bool includeChildren = false)
        {
            List<ValueTreeColumn> processingList = Columns.GetProcessingColumnList(columnNames);
            foreach (ValueTreeRow row in _rows)
            {
                foreach (ValueTreeColumn col in processingList)
                {
                    IValue current = row.Get(col);
                    if (value.Equals(current))
                        return row;
                }
                if (includeChildren)
                {
                    IValue childrenResult = row.Rows.Find(value, columnNames, includeChildren);
                    if (childrenResult.SystemType != BasicTypes.Undefined)
                    {
                        return childrenResult;
                    }
                }
            }
            return ValueFactory.Create();
        }

        private bool CheckFilterCriteria(ValueTreeRow row, StructureImpl filter)
        {
            foreach (KeyAndValueImpl kv in filter)
            {
                ValueTreeColumn column = Columns.FindColumnByName(kv.Key.AsString());
                if (column == null)
                    throw PropertyAccessException.PropNotFoundException(kv.Key.AsString());

                IValue current = row.Get(column);
                if (!current.Equals(kv.Value))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Ищет строки, отвечающие критериям отбора.
        /// </summary>
        /// <param name="filter">Структура. Структура, в которой Ключ - это имя колонки, а Значение - искомое значение.</param>
        /// <param name="includeChildren">Булево. Если Истина, в поиск будут включены все вложенные строки. Необязательный параметр.</param>
        /// <returns>Массив. Найденные строки.</returns>
        [ContextMethod("НайтиСтроки", "FindRows")]
        public ArrayImpl FindRows(IValue filter, bool includeChildren = false)
        {
            var filterStruct = filter.GetRawValue() as StructureImpl;

            if (filterStruct == null)
                throw RuntimeException.InvalidArgumentType();

            ArrayImpl result = new ArrayImpl();

            foreach (ValueTreeRow row in _rows)
            {
                if (CheckFilterCriteria(row, filterStruct))
                {
                    result.Add(row);
                }
                
                if (includeChildren)
                {
                    ArrayImpl childrenResult = row.Rows.FindRows(filter, includeChildren);
                    foreach (IValue value in childrenResult)
                    {
                        result.Add(value);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Удаляет все строки.
        /// </summary>
        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _rows.Clear();
        }

        /// <summary>
        /// Получает строку по индексу.
        /// </summary>
        /// <param name="index">Число. Индекс строки.</param>
        /// <returns>СтрокаДереваЗначений. Строка.</returns>
        [ContextMethod("Получить", "Get")]
        public ValueTreeRow Get(int index)
        {
            if (index < 0 || index >= Count())
                throw RuntimeException.InvalidArgumentValue();
            return _rows[index];
        }

        /// <summary>
        /// Сдвигает строку на указанное смещение.
        /// </summary>
        /// <param name="row">СтрокаДереваЗначений. Строка.</param>
        /// <param name="offset">Число. Смещение.</param>
        [ContextMethod("Сдвинуть", "Move")]
        public void Move(IValue row, int offset)
        {
            row = row.GetRawValue();

            int indexSource;
            if (row is ValueTreeRow)
                indexSource = _rows.IndexOf(row as ValueTreeRow);
            else if (row.SystemType == BasicTypes.Number)
                indexSource = decimal.ToInt32(row.AsNumber());
            else
                throw RuntimeException.InvalidArgumentType();

            if (indexSource < 0 || indexSource >= _rows.Count())
                throw RuntimeException.InvalidArgumentValue();

            int indexDestination = (indexSource + offset) % _rows.Count();
            while (indexDestination < 0)
                indexDestination += _rows.Count();

            ValueTreeRow tmp = _rows[indexSource];

            if (indexSource < indexDestination)
            {
                _rows.Insert(indexDestination + 1, tmp);
                _rows.RemoveAt(indexSource);
            }
            else
            {
                _rows.RemoveAt(indexSource);
                _rows.Insert(indexDestination, tmp);
            }

        }

        private struct ValueTreeSortRule
        {
            public ValueTreeColumn Column;
            public int direction; // 1 = asc, -1 = desc
        }

        private List<ValueTreeSortRule> GetSortRules(string columns)
        {

            string[] aColumns = columns.Split(',');

            List<ValueTreeSortRule> rules = new List<ValueTreeSortRule>();

            foreach (string column in aColumns)
            {
                string[] description = column.Trim().Split(' ');
                if (description.Count() == 0)
                    throw PropertyAccessException.PropNotFoundException(""); // TODO: WrongColumnNameException

                ValueTreeSortRule desc = new ValueTreeSortRule();
                desc.Column = this.Columns.FindColumnByName(description[0]);
                if (desc.Column == null)
                    throw PropertyAccessException.PropNotFoundException(description[0]);

                if (description.Count() > 1)
                {
                    if (String.Compare(description[1], "DESC", true) == 0 || String.Compare(description[1], "УБЫВ", true) == 0)
                        desc.direction = -1;
                    else
                        desc.direction = 1;
                }
                else
                    desc.direction = 1;

                rules.Add(desc);
            }

            return rules;
        }

        private class RowComparator : IComparer<ValueTreeRow>
        {
            readonly List<ValueTreeSortRule> _rules;

            readonly GenericIValueComparer _comparer = new GenericIValueComparer();

            public RowComparator(List<ValueTreeSortRule> rules)
            {
                if (rules.Count() == 0)
                    throw RuntimeException.InvalidArgumentValue();

                this._rules = rules;
            }

            private int OneCompare(ValueTreeRow x, ValueTreeRow y, ValueTreeSortRule rule)
            {
                IValue xValue = x.Get(rule.Column);
                IValue yValue = y.Get(rule.Column);

                int result = _comparer.Compare(xValue, yValue) * rule.direction;

                return result;
            }

            public int Compare(ValueTreeRow x, ValueTreeRow y)
            {
                int i = 0, r;
                while ((r = OneCompare(x, y, _rules[i])) == 0)
                {
                    if (++i >= _rules.Count())
                        return 0;
                }

                return r;
            }
        }

        /// <summary>
        /// Сортирует строки по указанному правилу.
        /// </summary>
        /// <param name="columns">Строка. Правило сортировки: список имён колонок, разделённых запятой. После имени через
        ///  пробел может указываться направление сортировки: Возр(Asc) - по возрастанию, Убыв(Desc) - по убыванию.</param>
        /// <param name="sortChildren">Булево. Если Истина, сортировка будет применена также к вложенным строкам.</param>
        /// <param name="comparator">СравнениеЗначений. Не используется.</param>
        [ContextMethod("Сортировать", "Sort")]
        public void Sort(string columns, bool sortChildren = false, IValue comparator = null)
        {
            Sort(new RowComparator(GetSortRules(columns)), sortChildren);
        }

        private void Sort(RowComparator comparator, bool sortChildren)
        {
            _rows.Sort(comparator);

            if (sortChildren)
            {
                foreach (var row in _rows)
                {
                    row.Rows.Sort(comparator, sortChildren);
                }
            }
        }

        /// <summary>
        /// Не поддерживается.
        /// </summary>
        [ContextMethod("ВыбратьСтроку", "ChooseRow")]
        public void ChooseRow(string title = null, IValue startRow = null)
        {
            throw new NotSupportedException();
        }

        internal void CopyFrom(ValueTreeRowCollection src)
        {
            _rows.Clear();
            ValueTreeColumnCollection columns = Owner().Columns;

            foreach (ValueTreeRow row in src._rows)
            {
                ValueTreeRow newRow = Add();
                foreach (ValueTreeColumn column in columns)
                {
                    newRow.Set(column, row.Get(ValueFactory.Create(column.Name)));
                }
                newRow.Rows.CopyFrom(row.Rows);
            }
        }


        public override IEnumerator<ValueTreeRow> GetEnumerator()
        {
            foreach (var item in _rows)
            {
                yield return item;
            }
        }


        public override IValue GetIndexedValue(IValue index)
        {
            return Get((int)index.AsNumber());
        }
    }
}
