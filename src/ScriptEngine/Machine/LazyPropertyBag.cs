/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace ScriptEngine.Machine
{
    /// <summary>
    /// Глобальные свойства окружения, у которых может быть действие перед первым чтением.
    /// Через него модуль библиотеки инициализируется, если тело другого модуля обратилось к нему раньше его очереди.
    /// </summary>
    internal class LazyPropertyBag : PropertyBag
    {
        // Действия перед первым чтением свойства и поток, задавший каждое из них
        private readonly Dictionary<int, (Action Initializer, int ThreadId)> _initializers = new Dictionary<int, (Action, int)>();
        private volatile int _initializersCount;

        public override IValue GetPropValue(int propNum)
        {
            if (_initializersCount != 0)
            {
                RunInitializer(propNum);
            }

            return base.GetPropValue(propNum);
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
        /// Выполняет действие свойства, если оно задано и еще не выполнялось.
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
    }
}
