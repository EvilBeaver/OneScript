/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.StandardLibrary.Collections.Exceptions;
using OneScript.StandardLibrary.TypeDescriptions;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.ValueTable
{
    /// <summary>
    /// Коллекция колонок таблицы значений
    /// </summary>
    [ContextClass("КоллекцияКолонокТаблицыЗначений", "ValueTableColumnCollection")]
    public class ValueTableColumnCollection : AutoContext<ValueTableColumnCollection>, ICollectionContext<ValueTableColumn>, IDebugPresentationAcceptor
    {
        private readonly List<ValueTableColumn> _columns = new List<ValueTableColumn>();
        private readonly StringComparer _namesComparer = StringComparer.OrdinalIgnoreCase;
        private readonly ValueTable _owner;
        private int maxColumnId = 0;

        public ValueTableColumnCollection(ValueTable owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Добавляет колонку в таблицу значений
        /// </summary>
        /// <param name="name">Строка - Имя колонки</param>
        /// <param name="type">ОписаниеТипов - Тип данных колонки</param>
        /// <param name="title">Строка - Заголовок колонки</param>
        /// <param name="width">Число - Ширина колонки</param>
        /// <returns>КолонкаТаблицыЗначений</returns>
        [ContextMethod("Добавить", "Add")]
        public ValueTableColumn Add(string name, TypeDescription type = null, string title = null, int width = 0)
        {
            if (!Utils.IsValidIdentifier(name))
                throw ColumnException.WrongColumnName(name);

            if (FindColumnByName(name) != null)
                throw ColumnException.DuplicatedColumnName(name);

            var column = new ValueTableColumn(this, ++maxColumnId, name, title, type, width);
            _columns.Add(column);

            return column;
        }

        /// <summary>
        /// Вставить колонку в указанную позицию
        /// </summary>
        /// <param name="index">Число - Индекс расположения колонки</param>
        /// <param name="name">Строка - Имя колонки</param>
        /// <param name="type">ОписаниеТипов - Тип данных колонки</param>
        /// <param name="title">Строка - Заголовок колонки</param>
        /// <param name="width">Число - Ширина колонки</param>
        /// <returns>КолонкаТаблицыЗначений</returns>
        [ContextMethod("Вставить", "Insert")]
        public ValueTableColumn Insert(int index, string name, TypeDescription type = null, string title = null, int width = 0)
        {
            if (!Utils.IsValidIdentifier(name))
                throw ColumnException.WrongColumnName(name);

            if (FindColumnByName(name) != null)
                throw ColumnException.DuplicatedColumnName(name);

            ValueTableColumn column = new ValueTableColumn(this, ++maxColumnId, name, title, type, width);
            _columns.Insert(index, column);

            return column;
        }

        /// <summary>
        /// Индекс указанной колонки
        /// </summary>
        /// <param name="column">КолонкаТаблицыЗначений - Колонка, для которой определяется индекс</param>
        /// <returns>Число</returns>
        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(ValueTableColumn column)
        {
            return _columns.IndexOf(column);
        }

        /// <summary>
        /// Количество колонок в таблице значений
        /// </summary>
        /// <returns>Число</returns>
        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _columns.Count;
        }
        
        public int Count(IBslProcess process) => Count();

        /// <summary>
        /// Поиск колонки по имени
        /// </summary>
        /// <param name="name">Строка - Имя колонки</param>
        /// <returns>КолонкаТаблицыЗначений - Найденная колонка таблицы значений, иначе Неопределено.</returns>
        [ContextMethod("Найти", "Find")]
        public IValue Find(string name)
        {
            ValueTableColumn Column = FindColumnByName(name);
            if (Column == null)
                return ValueFactory.Create();
            return Column;
        }

        /// <summary>
        /// Удалить колонку значений
        /// </summary>
        /// <param name="column">
        /// Строка - Имя колонки для удаления
        /// Число - Индекс колонки для удаления
        /// КолонкаТаблицыЗначений - Колонка для удаления
        /// </param>
        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue column)
        {
            var vtColumn = GetColumnByIIndex(column);
            _owner.ForEach((ValueTableRow x) =>
            {
                x.OnOwnerColumnRemoval(vtColumn);
            });
            _owner.Indexes.FieldRemoved(column);
            _columns.Remove(vtColumn);
        }

        /// <summary>
        /// Удаляет все колонки
        /// </summary>
        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            while (_columns.Count != 0)
            {
                Delete(_columns[0]);
            }
        }

        public ValueTableColumn FindColumnByName(string name)
        {
            return _columns.Find(column => _namesComparer.Equals(name, column.Name));
        }

        public ValueTableColumn FindColumnByIndex(int index)
        {
            if (index < 0 || index >= _columns.Count)
                throw RuntimeException.IndexOutOfRange();
            return _columns[index];
        }


        public IEnumerator<ValueTableColumn> GetEnumerator()
        {
            foreach (var item in _columns)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool IsIndexed => true;

        public override IValue GetIndexedValue(IValue index) => GetColumnByIIndex(index);
        
        public override int GetPropertyNumber(string name)
        {
            int idx = _columns.FindIndex(column => _namesComparer.Equals(name, column.Name));
            if (idx == -1)
                throw PropertyAccessException.PropNotFoundException(name);
            return idx;
        }

        public override int GetPropCount() => _columns.Count;

        public override string GetPropName(int propNum)
        {
            return FindColumnByIndex(propNum).Name;
        }

        public override IValue GetPropValue(int propNum)
        {
            return FindColumnByIndex(propNum);
        }

        public override bool IsPropWritable(int propNum) => false;

        public override bool IsPropReadable(int propNum) => true;

        public ValueTableColumn GetColumnByIIndex(IValue index)
        {
            if (index.SystemType == BasicTypes.String)
            {
                return FindColumnByName(index.ToString())
                    ?? throw PropertyAccessException.PropNotFoundException(index.ToString());
            }

            if (index.SystemType == BasicTypes.Number)
            {
                return FindColumnByIndex(decimal.ToInt32(index.AsNumber()));
            }

            if (index is ValueTableColumn column)
            {
                return column;
            }

            throw RuntimeException.InvalidArgumentType();
        }

        public int GetColumnNumericIndex(IValue index)
        {
            if (index.SystemType == BasicTypes.String)
            {
                return GetPropertyNumber(index.ToString());
            }

            if (index.SystemType == BasicTypes.Number)
            {
                int iIndex = Decimal.ToInt32(index.AsNumber());
                if (iIndex < 0 || iIndex >= Count())
                    throw RuntimeException.IndexOutOfRange();

                return iIndex;
            }

            if (index is ValueTableColumn column)
            {
                return IndexOf(column);
            }

            throw RuntimeException.InvalidArgumentType();
        }

        void IDebugPresentationAcceptor.Accept(IDebugValueVisitor visitor)
        {
            visitor.ShowProperties(this);
        }
    }
}
