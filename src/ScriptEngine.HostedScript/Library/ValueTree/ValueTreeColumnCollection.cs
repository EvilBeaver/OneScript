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
    [ContextClass("КоллекцияКолонокДереваЗначений", "ValueTreeColumnCollection")]
    class ValueTreeColumnCollection : DynamicPropertiesAccessor, ICollectionContext
    {
        private List<ValueTreeColumn> _columns = new List<ValueTreeColumn>();
        private int _internal_counter = 3; // Нарастающий счётчик определителей колонок
                                           // Начальное значение установлено в ненулевое для предопределённых полей строки дерева Родитель и Строки

        public ValueTreeColumnCollection()
        {
        }

        [ContextMethod("Добавить", "Add")]
        public ValueTreeColumn Add(string Name, IValue Type = null, string Title = null, int Width = 0)
        {
            if (FindColumnByName(Name) != null)
                throw new RuntimeException("Неверное имя колонки " + Name);

            ValueTreeColumn column = new ValueTreeColumn(this, ++_internal_counter, Name, Title, Type, Width);
            _columns.Add(column);

            return column;
        }

        [ContextMethod("Вставить", "Insert")]
        public ValueTreeColumn Insert(int index, string Name, IValue Type = null, string Title = null, int Width = 0)
        {
            if (FindColumnByName(Name) != null)
                throw new RuntimeException("Неверное имя колонки " + Name);

            ValueTreeColumn column = new ValueTreeColumn(this, ++_internal_counter, Name, Title, Type, Width);
            _columns.Insert(index, column);

            return column;
        }

        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(ValueTreeColumn column)
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
            ValueTreeColumn Column = FindColumnByName(Name);
            if (Column == null)
                return ValueFactory.Create();
            return Column;
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue Column)
        {
            Column = Column.GetRawValue();
            _columns.Remove(GetColumnByIIndex(Column));
        }

        [ContextMethod("Получить", "Get")]
        public ValueTreeColumn Get(int index)
        {
            if (index >= 0 && index < _columns.Count)
            {
                return _columns[index];
            }
            throw RuntimeException.InvalidArgumentValue();
        }

        internal void CopyFrom(ValueTreeColumnCollection src)
        {
            _columns.Clear();
            foreach (ValueTreeColumn column in src._columns)
            {
                _columns.Add(new ValueTreeColumn(this, ++_internal_counter, column));
            }
        }

        public ValueTreeColumn FindColumnByName(string Name)
        {
            var Comparer = StringComparer.OrdinalIgnoreCase;
            return _columns.Find(column => Comparer.Equals(Name, column.Name));
        }
        public ValueTreeColumn FindColumnById(int id)
        {
            return _columns.Find(column => column.ID == id);
        }

        public ValueTreeColumn FindColumnByIndex(int index)
        {
            return _columns[index];
        }

        public IEnumerator<IValue> GetEnumerator()
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
            ValueTreeColumn Column = FindColumnByName(name);
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

        public ValueTreeColumn GetColumnByIIndex(IValue index)
        {
            if (index.DataType == DataType.String)
            {
                ValueTreeColumn Column = FindColumnByName(index.AsString());
                if (Column == null)
                    throw RuntimeException.PropNotFoundException(index.AsString());
                return Column;
            }

            if (index.DataType == DataType.Number)
            {
                int i_index = Decimal.ToInt32(index.AsNumber());
                if (i_index < 0 || i_index >= Count())
                    throw RuntimeException.InvalidArgumentValue();

                ValueTreeColumn Column = FindColumnByIndex(i_index);
                return Column;
            }

            if (index is ValueTreeColumn)
            {
                return index as ValueTreeColumn;
            }

            throw RuntimeException.InvalidArgumentType();
        }

        public override IValue GetIndexedValue(IValue index)
        {
            return GetColumnByIIndex(index);
        }

        private static ContextMethodsMapper<ValueTreeColumnCollection> _methods = new ContextMethodsMapper<ValueTreeColumnCollection>();

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

        internal List<ValueTreeColumn> GetProcessingColumnList(string ColumnNames)
        {
            List<ValueTreeColumn> processing_list = new List<ValueTreeColumn>();
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
                    ValueTreeColumn Column = FindColumnByName(name.Trim());

                    if (Column == null)
                        throw RuntimeException.PropNotFoundException(name.Trim());

                    processing_list.Add(Column);
                }
            }
            else
            {
                foreach (ValueTreeColumn Column in _columns)
                    processing_list.Add(Column);
            }
            return processing_list;
        }

    }
}
