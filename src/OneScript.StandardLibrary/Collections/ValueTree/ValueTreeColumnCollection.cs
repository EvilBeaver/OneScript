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
using OneScript.Contexts;
using OneScript.StandardLibrary.TypeDescriptions;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.ValueTree
{
    /// <summary>
    /// Коллекция колонок дерева значений.
    /// </summary>
    [ContextClass("КоллекцияКолонокДереваЗначений", "ValueTreeColumnCollection", TypeUUID = "7FEEB150-ECAB-4971-865B-6CCBECC7D947")]
    public class ValueTreeColumnCollection : DynamicPropertiesAccessor, ICollectionContext, IEnumerable<ValueTreeColumn>, IDebugPresentationAcceptor
    {
        private readonly List<ValueTreeColumn> _columns = new List<ValueTreeColumn>();
        
        private static TypeDescriptor _instanceType = typeof(ValueTreeColumnCollection).GetTypeFromClassMarkup();
        private static readonly ContextMethodsMapper<ValueTreeColumnCollection> _methods = new ContextMethodsMapper<ValueTreeColumnCollection>();

        public ValueTreeColumnCollection()
        {
            DefineType(_instanceType);
        }

        /// <summary>
        /// Добавляет новую колонку.
        /// </summary>
        /// <param name="name">Строка. Имя колонки.</param>
        /// <param name="type">ОписаниеТипов. Доступные типы значений для колонки. Необязательный параметр.</param>
        /// <param name="title">Строка. Заголовок колонки. Необязательный параметр.</param>
        /// <param name="width">Число. Ширина колонки. Необязательный параметр.</param>
        /// <returns>КолонкаДереваЗначений. Добавленная колонка.</returns>
        [ContextMethod("Добавить", "Add")]
        public ValueTreeColumn Add(string name, TypeDescription type = null, string title = null, int width = 0)
        {
            if (FindColumnByName(name) != null)
                throw new RuntimeException("Неверное имя колонки " + name);

            ValueTreeColumn column = new ValueTreeColumn(this, name, title, type, width);
            _columns.Add(column);
            
            return column;
        }

        /// <summary>
        /// Вставляет новую колонку по указанному индексу.
        /// </summary>
        /// <param name="index">Число. Индекс новой колонки.</param>
        /// <param name="name">Строка. Имя колонки.</param>
        /// <param name="type">ОписаниеТипов. Доступные типы значений для колонки. Необязательный параметр.</param>
        /// <param name="title">Строка. Заголовок колонки. Необязательный параметр.</param>
        /// <param name="width">Число. Ширина колонки. Необязательный параметр.</param>
        /// <returns>КолонкаДереваЗначений. Добавленная колонка.</returns>
        [ContextMethod("Вставить", "Insert")]
        public ValueTreeColumn Insert(int index, string name, TypeDescription type = null, string title = null, int width = 0)
        {
            if (FindColumnByName(name) != null)
                throw new RuntimeException("Неверное имя колонки " + name);

            ValueTreeColumn column = new ValueTreeColumn(this, name, title, type, width);
            _columns.Insert(index, column);

            return column;
        }

        /// <summary>
        /// Определяет индекс колонки.
        /// </summary>
        /// <param name="column">КолонкаДереваЗначений. Колонка.</param>
        /// <returns>Число. Индекс колонки в коллекции. Если колонка не найдена, возвращается -1.</returns>
        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(ValueTreeColumn column)
        {
            return _columns.IndexOf(column);
        }

        /// <summary>
        /// Возвращает количество колонок.
        /// </summary>
        /// <returns>Число. Количество колонокs.</returns>
        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _columns.Count;
        }

        /// <summary>
        /// Ищет колонку по имени.
        /// </summary>
        /// <param name="name">Строка. Имя искомой колонки.</param>
        /// <returns>КолонкаДереваЗначений, Неопределено. Найденная колонка или Неопределено, если колонка не найдена.</returns>
        [ContextMethod("Найти", "Find")]
        public IValue Find(string name)
        {
            var column = FindColumnByName(name);
            if (column == null)
                return ValueFactory.Create();
            return column;
        }

        /// <summary>
        /// Удаляет колонку.
        /// </summary>
        /// <param name="column">КолонкаДереваЗначений. Колонка.</param>
        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue column)
        {
            column = column.GetRawValue();
            _columns.Remove(GetColumnByIIndex(column));
        }

        /// <summary>
        /// Получает колонку по индексу.
        /// </summary>
        /// <param name="index">Число. Индекс колонки.</param>
        /// <returns>КолонкаДереваЗначений. Колонка.</returns>
        [ContextMethod("Получить", "Get")]
        public ValueTreeColumn Get(int index)
        {
            if (index >= 0 && index < _columns.Count)
            {
                return _columns[index];
            }
            throw RuntimeException.InvalidArgumentValue();
        }

        /// <summary>
        /// Удаляет все колонки.
        /// </summary>
        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _columns.Clear();
        }

        /// <summary>
        /// Сдвигает колонку на указанное смещение.
        /// </summary>
        /// <param name="column">КолонкаДереваЗначений. Колонка.</param>
        /// <param name="offset">Число. Смещение.</param>
        [ContextMethod("Сдвинуть", "Move")]
        public void Move(IValue column, int offset)
        {
            var treeColumn = GetColumnByIIndex(column);
            int indexSource = _columns.IndexOf(treeColumn);

            int indexDestination = (indexSource + offset) % _columns.Count();
            while (indexDestination < 0)
            {
                indexDestination += _columns.Count();
            }


            if (indexSource < indexDestination)
            {
                _columns.Insert(indexDestination + 1, treeColumn);
                _columns.RemoveAt(indexSource);
            }
            else
            {
                _columns.RemoveAt(indexSource);
                _columns.Insert(indexDestination, treeColumn);
            }

        }

        internal void CopyFrom(ValueTreeColumnCollection src)
        {
            _columns.Clear();
            foreach (ValueTreeColumn column in src._columns)
            {
                _columns.Add(new ValueTreeColumn(this, column));
            }
        }

        public ValueTreeColumn FindColumnByName(string name)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            return _columns.Find(column => comparer.Equals(name, column.Name));
        }

        public ValueTreeColumn FindColumnByIndex(int index)
        {
            return _columns[index];
        }

        public IEnumerator<ValueTreeColumn> GetEnumerator()
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
            var column = FindColumnByName(name);
            if (column == null)
                throw PropertyAccessException.PropNotFoundException(name);
            return _columns.IndexOf(column);
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
            return _columns[propNum];
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
        }

        public ValueTreeColumn GetColumnByIIndex(IValue index)
        {
            if (index.SystemType == BasicTypes.String)
            {
                var column = FindColumnByName(index.AsString());
                if (column == null)
                    throw PropertyAccessException.PropNotFoundException(index.AsString());
                return column;
            }

            if (index.SystemType == BasicTypes.Number)
            {
                int indexNum = Decimal.ToInt32(index.AsNumber());
                if (indexNum < 0 || indexNum >= Count())
                    throw RuntimeException.InvalidArgumentValue();

                var column = FindColumnByIndex(indexNum);
                return column;
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

        public override BslMethodInfo GetRuntimeMethodInfo(int methodNumber)
        {
            return _methods.GetRuntimeMethod(methodNumber);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            var binding = _methods.GetCallableDelegate(methodNumber);
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
            var binding = _methods.GetCallableDelegate(methodNumber);
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

        internal List<ValueTreeColumn> GetProcessingColumnList(string columnNamesString)
        {
            List<ValueTreeColumn> processingList = new List<ValueTreeColumn>();
            if (columnNamesString != null)
            {

                if (columnNamesString.Trim().Length == 0)
                {
                    // Передали пустую строку вместо списка колонок
                    return processingList;
                }

                string[] columnNames = columnNamesString.Split(',');
                foreach (string name in columnNames)
                {
                    var column = FindColumnByName(name.Trim());

                    if (column == null)
                        throw PropertyAccessException.PropNotFoundException(name.Trim());

                    processingList.Add(column);
                }
            }
            else
            {
                foreach (var column in _columns)
                    processingList.Add(column);
            }
            return processingList;
        }

        void IDebugPresentationAcceptor.Accept(IDebugValueVisitor visitor)
        {
            visitor.ShowProperties(this);
        }
    }
}
