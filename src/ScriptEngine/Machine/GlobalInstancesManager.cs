/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.Machine
{
    public class GlobalInstancesManager : IGlobalsManager
    {
        // Экземпляры добавляются и во время работы (ПодключитьВнешнююКомпоненту, ЗагрузитьБиблиотеку),
        // пока другие потоки их читают
        private readonly ConcurrentDictionary<Type, object> _instances = new ConcurrentDictionary<Type, object>();

        public void Dispose()
        {
            foreach (var disposable in _instances
                .Select(x=>x.Value)                   
                .Where(x => x is IDisposable))
            {
                ((IDisposable)disposable).Dispose();
            }
            
            _instances.Clear();
        }

        public void RegisterInstance(object instance)
        {
            RegisterInstance(instance.GetType(), instance);
        }

        public void RegisterInstance(Type type, object instance)
        {
            if (!_instances.TryAdd(type, instance))
                throw new ArgumentException($"An item with the same key has already been added. Key: {type}");
        }

        public object GetInstance(Type type)
        {
            return _instances[type];
        }

        public T GetInstance<T>()
        {
            return (T)_instances[typeof(T)];
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            return _instances.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
