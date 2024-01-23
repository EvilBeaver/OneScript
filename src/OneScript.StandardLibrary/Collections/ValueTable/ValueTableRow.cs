﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.ValueTable
{
    [ContextClass("СтрокаТаблицыЗначений", "ValueTableRow", TypeUUID = "DBFCD195-4B87-4AB7-9BA7-AE2E791E04ED")]
    public class ValueTableRow : PropertyNameIndexAccessor, ICollectionContext<IValue>, IDebugPresentationAcceptor
    {
        private readonly Dictionary<IValue, IValue> _data = new Dictionary<IValue, IValue>();
        private readonly ValueTable _owner;

        private static readonly TypeDescriptor _objectType = typeof(ValueTableRow).GetTypeFromClassMarkup();
        
        public ValueTableRow(ValueTable owner) : base(_objectType)
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
            Set(C, value);
        }

        public void Set(IValue index, IValue value)
        {
            var C = Owner().Columns.GetColumnByIIndex(index);
            Set(C, value);
        }

        public void Set(ValueTableColumn column, IValue value)
        {
            Owner().Indexes.ElementRemoved(this);
            _data[column] = column.ValueType.AdjustValue(value);
            Owner().Indexes.ElementAdded(this);
        }

        public void OnOwnerColumnRemoval(IValue column)
        {
            _data.Remove(column);
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in Owner().Columns)
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
            return Owner().Columns.GetPropName(propNum);
        }

        public override int GetPropertyNumber(string name)
        {
            return Owner().Columns.GetPropertyNumber(name);
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
            var C = Owner().Columns.FindColumnByIndex(propNum);
            return TryValue(C);
        }

		public override void SetPropValue(int propNum, IValue newVal)
		{
			var C = Owner().Columns.FindColumnByIndex(propNum);
            Set(C, newVal);
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

        public override BslMethodInfo GetMethodInfo(int methodNumber)
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

        public override int GetMethodNumber(string name)
        {
            return _methods.FindMethod(name);
        }
        
        void IDebugPresentationAcceptor.Accept(IDebugValueVisitor visitor)
        {
            visitor.ShowProperties(this);
        }

    }
}
