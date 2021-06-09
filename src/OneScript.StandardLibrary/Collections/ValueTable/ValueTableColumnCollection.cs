/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Commons;
using OneScript.StandardLibrary.TypeDescriptions;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.ValueTable
{
    /// <summary>
    /// Коллекция колонок таблицы значений
    /// </summary>
    [ContextClass("КоллекцияКолонокТаблицыЗначений", "ValueTableColumnCollection", TypeUUID = "E1584766-C053-4644-B4C2-9642C0F53EFA")]
    public class ValueTableColumnCollection : DynamicPropertiesAccessor, ICollectionContext, IEnumerable<ValueTableColumn>, IDebugPresentationAcceptor
    {
        private readonly List<ValueTableColumn> _columns = new List<ValueTableColumn>();
        private readonly StringComparer _namesComparer = StringComparer.OrdinalIgnoreCase;
        private readonly ValueTable _owner;

        private static readonly TypeDescriptor _objectType = typeof(ValueTableColumnCollection).GetTypeFromClassMarkup();

        public ValueTableColumnCollection(ValueTable owner)
        {
            _owner = owner;
            DefineType(_objectType);
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
            if (FindColumnByName(name) != null)
                throw new RuntimeException("Неверное имя колонки " + name);

            var column = new ValueTableColumn(this, name, title, type, width);
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
            if (FindColumnByName(name) != null)
                throw new RuntimeException("Неверное имя колонки " + name);

            ValueTableColumn column = new ValueTableColumn(this, name, title, type, width);
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
            column = column.GetRawValue();
            var vtColumn = GetColumnByIIndex(column);
            _owner.ForEach(x=>x.OnOwnerColumnRemoval(vtColumn));
            _columns.Remove(vtColumn);
        }

        public ValueTableColumn FindColumnByName(string name)
        {
            return _columns.Find(column => _namesComparer.Equals(name, column.Name));
        }

        public ValueTableColumn FindColumnByIndex(int index)
        {
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

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        public override int FindProperty(string name)
        {
            int idx = _columns.FindIndex(column => _namesComparer.Equals(name, column.Name));
            if (idx == -1)
                throw PropertyAccessException.PropNotFoundException(name);
            return idx;
        }

        public override int GetPropCount()
        {
            return _columns.Count;
        }
        
        public override string GetPropName(int propNum)
        {
            return FindColumnByIndex(propNum).Name;
        }

        public override IValue GetPropValue(int propNum)
        {
            return FindColumnByIndex(propNum);
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
        }

        public ValueTableColumn GetColumnByIIndex(IValue index)
        {
            if (index.SystemType == BasicTypes.String)
            {
                ValueTableColumn Column = FindColumnByName(index.AsString());
                if (Column == null)
                    throw PropertyAccessException.PropNotFoundException(index.AsString());
                return Column;
            }

            if (index.SystemType == BasicTypes.Number)
            {
                int i_index = Decimal.ToInt32(index.AsNumber());
                if (i_index < 0 || i_index >= Count())
                    throw RuntimeException.InvalidArgumentValue();

                ValueTableColumn Column = FindColumnByIndex(i_index);
                return Column;
            }

            if (index is ValueTableColumn)
            {
                return index as ValueTableColumn;
            }

            throw RuntimeException.InvalidArgumentType();
        }

        public int GetColumnNumericIndex(IValue index)
        {
            if (index.SystemType == BasicTypes.String)
            {
                return FindProperty(index.AsString());
            }

            if (index.SystemType == BasicTypes.Number)
            {
                int iIndex = Decimal.ToInt32(index.AsNumber());
                if (iIndex < 0 || iIndex >= Count())
                    throw RuntimeException.InvalidArgumentValue();

                return iIndex;
            }

            var column = index.GetRawValue() as ValueTableColumn;
            if (column != null)
            {
                return IndexOf(column);
            }

            throw RuntimeException.InvalidArgumentType();
        }

        public override IValue GetIndexedValue(IValue index)
        {
            return GetColumnByIIndex(index);
        }

        private static readonly ContextMethodsMapper<ValueTableColumnCollection> _methods = new ContextMethodsMapper<ValueTableColumnCollection>();

        public override MethodSignature GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodSignature(methodNumber);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            var binding = _methods.GetCallableDelegate(methodNumber);
            try
            {
                binding(this, arguments);
            } catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            var binding = _methods.GetCallableDelegate(methodNumber);
            try
            {
                retValue = binding(this, arguments);
            } catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }

        void IDebugPresentationAcceptor.Accept(IDebugValueVisitor visitor)
        {
            visitor.ShowProperties(this);
        }
    }
}
