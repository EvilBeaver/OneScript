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
    [ContextClass("СтрокаТаблицыЗначений", "ValueTableRow")]
    public class ValueTableRow : PropertyNameIndexAccessor, ICollectionContext, IEnumerable<IValue>
    {
        private readonly Dictionary<IValue, IValue> _data = new Dictionary<IValue, IValue>();
        private readonly ValueTable _owner;

        public ValueTableRow(ValueTable owner)
        {
            _owner = owner;
        }

        public int Count()
        {
            return Owner().Columns.Count();
        }

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
			IValue Value;
			if (_data.TryGetValue(Column, out Value))
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
            var C = Owner().Columns.FindColumnByIndex(index);
            return TryValue(C);
        }

        public IValue Get(IValue index)
        {
            var C = Owner().Columns.GetColumnByIIndex(index);
            return TryValue(C);
        }

        public IValue Get(ValueTableColumn c)
        {
            return TryValue(c);
        }
        
        /// <summary>
        /// Установить значение
        /// </summary>
        /// <param name="index">Число - Индекс колонки</param>
        /// <param name="value">Произвольный - значение для установки</param>
        [ContextMethod("Установить", "Set")]
        public void Set(int index, IValue value)
        {
            var C = Owner().Columns.FindColumnByIndex(index);
            _data[C] = C.ValueType.AdjustValue(value);
        }

        public void Set(IValue index, IValue value)
        {
            var C = Owner().Columns.GetColumnByIIndex(index);
            _data[C] = C.ValueType.AdjustValue(value);
        }

        public void Set(ValueTableColumn column, IValue value)
        {
            _data[column] = column.ValueType.AdjustValue(value);
        }

        public void OnOwnerColumnRemoval(IValue column)
        {
            _data.Remove(column);
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (ValueTableColumn item in Owner().Columns)
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

        public override int GetPropCount()
        {
            return Count();
        }

        public override string GetPropName(int propNum)
        {
            return Owner().Columns.GetPropName(propNum);
        }

        public override int FindProperty(string name)
        {
            ValueTableColumn C = Owner().Columns.FindColumnByName(name);
            
            if (C == null)
                throw RuntimeException.PropNotFoundException(name);

            return C.ID;
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
            ValueTableColumn C = Owner().Columns.FindColumnById(propNum);
            return TryValue(C);
        }

		public override void SetPropValue(int propNum, IValue newVal)
		{
			ValueTableColumn C = Owner().Columns.FindColumnById(propNum);
			_data[C] = C.ValueType.AdjustValue(newVal);
		}

        private ValueTableColumn GetColumnByIIndex(IValue index)
        {
            return Owner().Columns.GetColumnByIIndex(index);
        }

        public override IValue GetIndexedValue(IValue index)
        {
            ValueTableColumn C = GetColumnByIIndex(index);
            return TryValue(C);
        }

		public override void SetIndexedValue(IValue index, IValue val)
		{
			var C = GetColumnByIIndex(index);
			_data[C] = C.ValueType.AdjustValue(val);
		}


        private static readonly ContextMethodsMapper<ValueTableRow> _methods = new ContextMethodsMapper<ValueTableRow>();

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
