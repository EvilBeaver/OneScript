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
    [ContextClass("КоллекцияКолонокТаблицыЗначений", "ValueTableColumnCollection")]
    public class ValueTableColumnCollection : DynamicPropertiesAccessor, ICollectionContext, IEnumerable<ValueTableColumn>
    {
        private readonly List<ValueTableColumn> _columns = new List<ValueTableColumn>();
        private int _internal_counter = 0; // Нарастающий счётчик определителей колонок

        private readonly ValueTable _owner;

        public ValueTableColumnCollection(ValueTable owner)
        {
            _owner = owner;
        }

        [ContextMethod("Добавить", "Add")]
        public ValueTableColumn Add(string Name, IValue Type = null, string Title = null)
        {
            if (FindColumnByName(Name) != null)
                throw new RuntimeException("Неверное имя колонки " + Name);

            var Width = 0; // затычка

            ValueTableColumn column = new ValueTableColumn(this, ++_internal_counter, Name, Title, Type, Width);
            _columns.Add(column);

            return column;
        }

        [ContextMethod("Вставить", "Insert")]
        public ValueTableColumn Insert(int index, string Name, IValue Type = null)
            // TODO: добавить Title и Width после того, как количество обрабатываемых параметров будет увеличено хотя бы до 5
        {
            if (FindColumnByName(Name) != null)
                throw new RuntimeException("Неверное имя колонки " + Name);

            var Title = Name; // TODO: Затычка
            var Width = 0; // TODO: Затычка

            ValueTableColumn column = new ValueTableColumn(this, ++_internal_counter, Name, Title, Type, Width);
            _columns.Insert(index, column);

            return column;
        }

        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(ValueTableColumn column)
        {
            return _columns.IndexOf(column);
        }

        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _columns.Count;
        }

        [ContextMethod("Найти", "Find")]
        public IValue Find(string Name)
        {
            ValueTableColumn Column = FindColumnByName(Name);
            if (Column == null)
                return ValueFactory.Create();
            return Column;
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue Column)
        {
            Column = Column.GetRawValue();
            var vtColumn = GetColumnByIIndex(Column);
            _owner.ForEach(x=>x.OnOwnerColumnRemoval(vtColumn));
            _columns.Remove(vtColumn);
        }

        public ValueTableColumn FindColumnByName(string Name)
        {
            var Comparer = StringComparer.OrdinalIgnoreCase;
            return _columns.Find(column => Comparer.Equals(Name, column.Name));
        }
        public ValueTableColumn FindColumnById(int id)
        {
            return _columns.Find(column => column.ID == id);
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
            ValueTableColumn Column = FindColumnByName(name);
            if (Column == null)
                throw RuntimeException.PropNotFoundException(name);
            return Column.ID;
        }

        public override IValue GetPropValue(int propNum)
        {
            return FindColumnById(propNum);
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
        }

        public ValueTableColumn GetColumnByIIndex(IValue index)
        {
            if (index.DataType == DataType.String)
            {
                ValueTableColumn Column = FindColumnByName(index.AsString());
                if (Column == null)
                    throw RuntimeException.PropNotFoundException(index.AsString());
                return Column;
            }

            if (index.DataType == DataType.Number)
            {
                int i_index = Decimal.ToInt32(index.AsNumber());
                if (i_index < 0 || i_index >= Count())
                    throw RuntimeException.InvalidArgumentValue();

                ValueTableColumn Column = FindColumnByIndex(i_index);
                return Column;
            }

            if (index is ValueTableColumn) {
                return index as ValueTableColumn;
            }

            throw RuntimeException.InvalidArgumentType();
        }

        public int GetColumnNumericIndex(IValue index)
        {
            if (index.DataType == DataType.String)
            {
                ValueTableColumn Column = FindColumnByName(index.AsString());
                if (Column == null)
                    throw RuntimeException.PropNotFoundException(index.AsString());
                return Column.ID;
            }

            if (index.DataType == DataType.Number)
            {
                int iIndex = Decimal.ToInt32(index.AsNumber());
                if (iIndex < 0 || iIndex >= Count())
                    throw RuntimeException.InvalidArgumentValue();

                return iIndex;
            }

            var column = index.GetRawValue() as ValueTableColumn;
            if (column != null)
            {
                return column.ID;
            }

            throw RuntimeException.InvalidArgumentType();
        }

        public override IValue GetIndexedValue(IValue index)
        {
            return GetColumnByIIndex(index);
        }

        private static readonly ContextMethodsMapper<ValueTableColumnCollection> _methods = new ContextMethodsMapper<ValueTableColumnCollection>();

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            var binding = _methods.GetMethod(methodNumber);
            try
            {
                binding(this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            var binding = _methods.GetMethod(methodNumber);
            try
            {
                retValue = binding(this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }
    }
}
