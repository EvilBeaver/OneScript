/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Execution;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.ValueTable
{
    [ContextClass("СтрокаТаблицыЗначений", "ValueTableRow")]
    public class ValueTableRow : AutoContext<ValueTableRow>, ICollectionContext<IValue>, IDebugPresentationAcceptor
    {
        private readonly Dictionary<ValueTableColumn, IValue> _data = new Dictionary<ValueTableColumn, IValue>();
        private readonly ValueTable _owner;

        public ValueTableRow(ValueTable owner)
        {
            _owner = owner;
        }

        public int Count()
        {
            return _owner.Columns.Count();
        }
        
        public int Count(IBslProcess process) => Count();
        
        /// <summary>
        /// Владелец строки
        /// </summary>
        /// <returns>ТаблицаЗначений</returns>
        [ContextMethod("Владелец", "Owner")]
        public ValueTable Owner()
        {
            return _owner;
        }

		private IValue TryValue(ValueTableColumn Column)
		{
            if (_data.TryGetValue(Column, out IValue Value))
            {
                return Value;
            }
            return Column.ValueType.AdjustValue();
		}

        /// <summary>
        /// Получает значение по индексу
        /// </summary>
        /// <param name="index">Число - Индекс колонки</param>
        /// <returns>Произвольный - Значение колонки</returns>
        [ContextMethod("Получить", "Get")]
        public IValue Get(int index)
        {
            return TryValue(_owner.Columns.FindColumnByIndex(index));
        }

        public IValue Get(IValue index)
        {
            return TryValue(_owner.Columns.GetColumnByIIndex(index));
        }

        public IValue Get(ValueTableColumn column)
        {
            return TryValue(column);
        }
        
        /// <summary>
        /// Установить значение
        /// </summary>
        /// <param name="index">Число - Индекс колонки</param>
        /// <param name="value">Произвольный - значение для установки</param>
        [ContextMethod("Установить", "Set")]
        public void Set(int index, IValue value)
        {
            Set(_owner.Columns.FindColumnByIndex(index), value);
        }

        public void Set(IValue index, IValue value)
        {
            Set(_owner.Columns.GetColumnByIIndex(index), value);
        }

        public void Set(ValueTableColumn column, IValue value)
        {
            _owner.Indexes.ElementRemoved(this);
            _data[column] = column.ValueType.AdjustValue(value);
            _owner.Indexes.ElementAdded(this);
        }

        public void OnOwnerColumnRemoval(ValueTableColumn column)
        {
            _data.Remove(column);
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _owner.Columns)
            {
                yield return TryValue(item);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override int GetPropCount()
        {
            return Count();
        }

        public override string GetPropName(int propNum)
        {
            return _owner.Columns.GetPropName(propNum);
        }

        public override int GetPropertyNumber(string name)
        {
            return _owner.Columns.GetPropertyNumber(name);
        }

        public override bool IsPropReadable(int propNum) => true;

        public override bool IsPropWritable(int propNum) => true; 

        public override IValue GetPropValue(int propNum)
        {
            return TryValue(_owner.Columns.FindColumnByIndex(propNum));
        }

		public override void SetPropValue(int propNum, IValue newVal)
		{
            Set(_owner.Columns.FindColumnByIndex(propNum), newVal);
		}

        private ValueTableColumn GetColumnByIIndex(IValue index)
        {
            return _owner.Columns.GetColumnByIIndex(index);
        }

        public override IValue GetIndexedValue(IValue index)
        {
            return TryValue(GetColumnByIIndex(index));
        }

		public override void SetIndexedValue(IValue index, IValue val)
		{
            Set(GetColumnByIIndex(index), val);
		}

        void IDebugPresentationAcceptor.Accept(IDebugValueVisitor visitor)
        {
            visitor.ShowProperties(this);
        }

    }
}
