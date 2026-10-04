/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    internal class PropertyBag : DynamicPropertiesAccessor, IAttachableContext
    {
        private readonly List<IValue> _values = new List<IValue>();

        // Инициализаторы, которые выполняются перед первым чтением свойства, и поток, задавший каждый из них
        private readonly Dictionary<int, (Action Initializer, int ThreadId)> _initializers = new Dictionary<int, (Action, int)>();
        private volatile int _initializersCount;

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
            if (_initializersCount != 0)
            {
                RunInitializer(propNum);
            }

            return _values[propNum];
        }

        /// <summary>
        /// Задает действие, которое выполнится перед первым чтением свойства.
        /// Выполняет его только поток, который его задал, остальные получают значение как есть.
        /// </summary>
        public void SetInitializer(int propNum, Action initializer)
        {
            lock (_initializers)
            {
                _initializers[propNum] = (initializer, Environment.CurrentManagedThreadId);
                _initializersCount = _initializers.Count;
            }
        }

        /// <summary>
        /// Выполняет инициализатор свойства, если он задан и еще не выполнялся.
        /// </summary>
        public void RunInitializer(int propNum)
        {
            Action initializer;
            lock (_initializers)
            {
                if (!_initializers.TryGetValue(propNum, out var pending)
                    || pending.ThreadId != Environment.CurrentManagedThreadId)
                {
                    return;
                }

                // Убираем до вызова: при круговом обращении второй получит значение без инициализации
                _initializers.Remove(propNum);
                _initializersCount = _initializers.Count;
                initializer = pending.Initializer;
            }

            initializer();
        }

        public void RemoveInitializer(int propNum)
        {
            lock (_initializers)
            {
                _initializers.Remove(propNum);
                _initializersCount = _initializers.Count;
            }
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            _values[propNum] = newVal;
        }
        
        public int Count => _values.Count;

        public override int GetMethodsCount()
        {
            return 0;
        }

        #region IAttachableContext Members

        IVariable IAttachableContext.GetVariable(int index) => 
            Variable.CreateContextPropertyReference(this, index, GetPropertyName(index));
        
        BslMethodInfo IAttachableContext.GetMethod(int index) => throw new ArgumentOutOfRangeException();

        int IAttachableContext.VariablesCount => this.Count;
        
        int IAttachableContext.MethodsCount => 0;

        #endregion
    }
}
