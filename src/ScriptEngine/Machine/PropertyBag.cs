/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    internal class PropertyBag : DynamicPropertiesAccessor, IAttachableContext
    {
        private readonly List<IValue> _values = new List<IValue>();
        private readonly HashSet<int> _checkedDeprecatedProps = new HashSet<int>();

        public void Insert(IValue value, string identifier)
        {
            Insert(value, identifier, true, true);
        }

        public int Insert(IValue value, string identifier, bool canRead, bool canWrite)
        {
            var num = RegisterProperty(identifier, canRead, canWrite);

            if (num == _values.Count)
            {
                _values.Add(null);
            }

            value ??= ValueFactory.Create();

            SetPropValue(num, value);

            return num;
        }

        public int Insert(IValue value, BslPropertyInfo definition)
        {
            var num = RegisterProperty(definition);
            if (num == _values.Count)
            {
                _values.Add(null);
            }

            value ??= ValueFactory.Create();

            _values[num] = value;

            return num;
        }

        public override bool IsPropReadable(int propNum)
        {
            return GetPropertyInfo(propNum).CanRead;
        }

        public override bool IsPropWritable(int propNum)
        {
            return GetPropertyInfo(propNum).CanWrite;
        }

        public override IValue GetPropValue(int propNum)
        {
            // Важно: любое чтение значения глобального свойства проходит через этот метод и вызывает WarnDeprecation.
            // В отладке визуализатор переменных (DefaultVariableVisualizer) может косвенно инициировать чтение значений
            // для построения представления (например, через IsStructured/Value.ToString()), что приведет к логированию
            // предупреждения об обращении к устаревшему свойству, даже если пользовательский код его не читал.
            WarnDeprecation(propNum);
            return _values[propNum];
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            // Аналогично чтению: запись также фиксируется для устаревших свойств один раз за сеанс.
            WarnDeprecation(propNum);
            _values[propNum] = newVal;
        }

        /// <summary>
        /// Логирует предупреждение об обращении к устаревшему системному свойству.
        /// Особенности:
        /// - Предупреждение выводится один раз для каждого свойства за время жизни PropertyBag (см. _checkedDeprecatedProps).
        /// - В режиме отладки предупреждение может появляться «ложно» при остановке на брейкпоинте, если
        ///   визуализатор переменных (OneScript.DebugServices.DefaultVariableVisualizer) читает значение
        ///   для построения UI (например, через variable.Value в IsStructured или value.ToString()).
        ///   Это чтение также проходит через GetPropValue и попадает сюда.
        /// - Реальные обращения из пользовательского кода также фиксируются здесь.
        /// См. также:
        ///   - OneScript.StandardLibrary.Binary.StreamEnums.StreamPositionEnum помечен [DeprecatedName("StreamPosition")],
        ///     что делает алиас устаревшим и провоцирует предупреждение при первом чтении.
        /// </summary>
        private void WarnDeprecation(int propNum)
        {
            if (DeprecationWarningScope.IsSuppressed)
                return;

            if (_checkedDeprecatedProps.Contains(propNum)) 
                return;

            if (GetPropertyInfo(propNum) is SystemPropertyInfo { IsDeprecated: true })
            {
                SystemLogger.Write($"Обращение к устаревшему свойству {GetPropertyInfo(propNum).Name}.");
            }

            _checkedDeprecatedProps.Add(propNum);
        }

        public int Count => _values.Count;

        public override int GetMethodsCount()
        {
            return 0;
        }

        #region IAttachableContext Members

        public void OnAttach(out IVariable[] variables, out BslMethodInfo[] methods)
        {
            variables = new IVariable[this.Count];
            var props = GetDynamicProperties().OrderBy(x => x.Value).Select(x=>x.Key).ToArray();
            Debug.Assert(props.Length == variables.Length);

            for (var i = 0; i < variables.Length; i++)
            {
                variables[i] = Variable.CreateContextPropertyReference(this, i, props[i]);
            }

            methods = Array.Empty<BslMethodInfo>();
        }

        #endregion
    }
}
