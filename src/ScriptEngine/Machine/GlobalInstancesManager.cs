/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.Machine
{
    public class GlobalInstancesManager : IGlobalsManager
    {
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

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
            _instances.Add(instance.GetType(), instance);
        }

        public void RegisterInstance(Type type, object instance)
        {
            _instances.Add(type, instance);
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
