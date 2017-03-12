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
    /// <summary>
    /// Строка дерева значений.
    /// </summary>
    [ContextClass("СтрокаДереваЗначений", "ValueTreeRow")]
    public class ValueTreeRow : PropertyNameIndexAccessor, ICollectionContext, IEnumerable<IValue>
    {
        private readonly Dictionary<IValue, IValue> _data = new Dictionary<IValue, IValue>();
        private readonly ValueTreeRow _parent;
        private readonly ValueTree _owner;
        private readonly int _level;
        private readonly ValueTreeRowCollection _rows;

        public ValueTreeRow(ValueTree owner, ValueTreeRow parent, int level)
        {
            _owner = owner;
            _parent = parent;
            _level = level;
            _rows = new ValueTreeRowCollection(owner, this, level + 1);
        }

        public int Count()
        {
            return _owner.Columns.Count();
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

        [ContextProperty("Строки", "Rows")]
        public ValueTreeRowCollection Rows
        {
            get { return _rows; }
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

        private IValue TryValue(ValueTreeColumn column)
        {
            IValue value;
            if (_data.TryGetValue(column, out value))
                return value;
            return ValueFactory.Create(); // TODO: Определять пустое значение для типа колонки
        }

        /// <summary>
        /// Получает значение по индексу.
        /// </summary>
        /// <param name="index">Число. Индекс получаемого параметра.</param>
        /// <returns>Произвольный. Получаемое значение.</returns>
        [ContextMethod("Получить", "Get")]
        public IValue Get(int index)
        {
            var column = Owner().Columns.FindColumnByIndex(index);
            return TryValue(column);
        }

        public IValue Get(IValue index)
        {
            var column = Owner().Columns.GetColumnByIIndex(index);
            return TryValue(column);
        }

        public IValue Get(ValueTreeColumn column)
        {
            return TryValue(column);
        }

        /// <summary>
        /// Устанавливает значение по индексу.
        /// </summary>
        /// <param name="index">Число. Индекс параметра, которому задаётся значение.</param>
        /// <param name="value">Произвольный. Новое значение.</param>
        [ContextMethod("Установить", "Set")]
        public void Set(int index, IValue value)
        {
            var column = Owner().Columns.FindColumnByIndex(index);
            _data[column] = value;
        }

        public void Set(IValue index, IValue value)
        {
            var column = Owner().Columns.GetColumnByIIndex(index);
            _data[column] = value;
        }

        public void Set(ValueTreeColumn column, IValue value)
        {
            _data[column] = value;
        }

        /// <summary>
        /// Возвращает уровень вложенности строки в дереве.
        /// Строки верхнего уровня имеют значение 0.
        /// </summary>
        /// <returns>Число. Уровень вложенности строки.</returns>
        [ContextMethod("Уровень", "Level")]
        public int Level()
        {
            return _level;
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (ValueTreeColumn item in Owner().Columns)
            {
                yield return TryValue(item);
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

        private static readonly ContextPropertyMapper<ValueTreeRow> _properties = new ContextPropertyMapper<ValueTreeRow>();

        public override int FindProperty(string name)
        {
            var column = Owner().Columns.FindColumnByName(name);

            if (column == null)
            {
                return _properties.FindProperty(name);
            }

            return column.ID;
        }

        public override bool IsPropReadable(int propNum)
        {
            return true;
        }

        public override bool IsPropWritable(int propNum)
        {
            return true;
        }

        public override IValue GetPropValue(int propNum)
        {
            var column = Owner().Columns.FindColumnById(propNum);
            if (column == null)
            {
                var property = _properties.GetProperty(propNum);
                return property.Getter(this);
            }
            return TryValue(column);
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            var column = Owner().Columns.FindColumnById(propNum);
            if (column == null)
            {
                var property = _properties.GetProperty(propNum);
                property.Setter(this, newVal);
            }
            else
            {
                _data[column] = newVal;
            }
        }

        private ValueTreeColumn GetColumnByIIndex(IValue index)
        {
            return Owner().Columns.GetColumnByIIndex(index);
        }

        public override IValue GetIndexedValue(IValue index)
        {
            var column = GetColumnByIIndex(index);
            return TryValue(column);
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            _data[GetColumnByIIndex(index)] = val;
        }


        private static readonly ContextMethodsMapper<ValueTreeRow> _methods = new ContextMethodsMapper<ValueTreeRow>();

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
