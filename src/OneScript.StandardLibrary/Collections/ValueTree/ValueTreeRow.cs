/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.ValueTree
{
    /// <summary>
    /// Строка дерева значений.
    /// </summary>
    [ContextClass("СтрокаДереваЗначений", "ValueTreeRow", TypeUUID = "4EE26F99-54A8-4640-B2F8-3DA1CB102113")]
    public class ValueTreeRow : PropertyNameIndexAccessor, ICollectionContext, IEnumerable<IValue>, IDebugPresentationAcceptor
    {
        private readonly Dictionary<IValue, IValue> _data = new Dictionary<IValue, IValue>();
        private readonly ValueTreeRow _parent;
        private readonly ValueTree _owner;
        private readonly int _level;
        private readonly ValueTreeRowCollection _rows;
        
        private static TypeDescriptor _instanceType = typeof(ValueTreeRow).GetTypeFromClassMarkup();
        private static readonly ContextPropertyMapper<ValueTreeRow> _properties = new ContextPropertyMapper<ValueTreeRow>();
        private static readonly ContextMethodsMapper<ValueTreeRow> _methods = new ContextMethodsMapper<ValueTreeRow>();

        public ValueTreeRow(ValueTree owner, ValueTreeRow parent, int level)
            : base(_instanceType)
        {
            _owner = owner;
            _parent = parent;
            _level = level;
            _rows = new ValueTreeRowCollection(owner, this, level + 1);
        }

        public int Count()
        {
            return _properties.Count + _owner.Columns.Count();
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
			{
				return value;
			}
			return column.ValueType.AdjustValue();
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
			_data[column] = column.ValueType.AdjustValue(value);
		}

		public void Set(IValue index, IValue value)
		{
			var column = Owner().Columns.GetColumnByIIndex(index);
			_data[column] = column.ValueType.AdjustValue(value);
		}

		public void Set(ValueTreeColumn column, IValue value)
		{
			_data[column] = column.ValueType.AdjustValue(value);
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

        public override int GetPropCount()
        {
            return Count();
        }

        public override string GetPropName(int propNum)
        {
            if (IsOwnProp(propNum))
                return _properties.GetProperty(propNum).Name;
            return Owner().Columns.GetPropName(GetColumnIndex(propNum));
        }

        public override int FindProperty(string name)
        {
            if(_properties.ContainsProperty(name))
                return _properties.FindProperty(name);

            var cols = Owner().Columns;
            var column = cols.FindColumnByName(name);
            
            if(column == null)
                throw OldRuntimeException.PropNotFoundException(name);

            return GetColumnPropIndex(cols.IndexOf(column));
        }

        public override bool IsPropReadable(int propNum)
        {
            return true;
        }

        public override bool IsPropWritable(int propNum)
        {
            return !IsOwnProp(propNum);
        }

        public override IValue GetPropValue(int propNum)
        {
            if (IsOwnProp(propNum))
            {
                var property = _properties.GetProperty(propNum);
                return property.Getter(this);
            }
            var column = Owner().Columns.FindColumnByIndex(GetColumnIndex(propNum));
            return TryValue(column);
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            if (IsOwnProp(propNum))
            {
                var property = _properties.GetProperty(propNum);
                property.Setter(this, newVal);
                return;
            }
            var column = Owner().Columns.FindColumnByIndex(GetColumnIndex(propNum));
            _data[column] = column.ValueType.AdjustValue(newVal);
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
            var column = GetColumnByIIndex(index);
            _data[GetColumnByIIndex(index)] = column.ValueType.AdjustValue(val);
        }

        private static bool IsOwnProp(int propNum)
        {
            return _properties.Count - 1 >= propNum;
        }

        private static int GetColumnPropIndex(int index)
        {
            return _properties.Count + index;
        }

        private static int GetColumnIndex(int propIndex)
        {
            return propIndex - _properties.Count;
        }

        public override MethodSignature GetMethodInfo(int methodNumber)
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
        
        void IDebugPresentationAcceptor.Accept(IDebugValueVisitor visitor)
        {
            visitor.ShowProperties(this);
        }
        
    }
}
