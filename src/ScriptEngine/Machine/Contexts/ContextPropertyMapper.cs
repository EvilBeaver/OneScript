/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Types;

namespace ScriptEngine.Machine.Contexts
{
    public class PropertyTarget<TInstance>
    {
        private readonly BslPropertyInfo _propertyInfo;

        public PropertyTarget(PropertyInfo propInfo)
        {
            _propertyInfo = new ContextPropertyInfo(propInfo);
            Name = _propertyInfo.Name;
            Alias = _propertyInfo.Alias;
            
            if (string.IsNullOrEmpty(Alias))
                Alias = propInfo.Name;

            IValue CantReadAction(TInstance inst)
            {
                throw PropertyAccessException.PropIsNotReadableException(Name);
            }

            void CantWriteAction(TInstance inst, IValue val)
            {
                throw PropertyAccessException.PropIsNotWritableException(Name);
            }

            if (_propertyInfo.CanRead)
            {
                var getMethodInfo = propInfo.GetGetMethod();
                if (getMethodInfo == null)
                {
                    Getter = CantReadAction;
                }
                else
                {
                    var genericGetter = typeof(PropertyTarget<TInstance>).GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(x => x.MemberType == MemberTypes.Method && x.Name == nameof(CreateGetter))
                        .Select(x => (MethodInfo)x)
                        .First();

                    var resolvedGetter = genericGetter.MakeGenericMethod(propInfo.PropertyType);

                    Getter = (Func<TInstance, IValue>)resolvedGetter.Invoke(this, new object[] { getMethodInfo });
                }
            }
            else
            {
                Getter = CantReadAction;
            }

            if (_propertyInfo.CanWrite)
            {
                var setMethodInfo = propInfo.GetSetMethod();
                if (setMethodInfo == null)
                {
                    Setter = CantWriteAction;
                }
                else
                {
                    var genericSetter = typeof(PropertyTarget<TInstance>).GetMembers(BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(x => x.MemberType == MemberTypes.Method && x.Name == nameof(CreateSetter))
                        .Select(x => (MethodInfo)x)
                        .First();

                    var resolvedSetter = genericSetter.MakeGenericMethod(propInfo.PropertyType);

                    Setter = (Action<TInstance, IValue>)resolvedSetter.Invoke(this, new object[] { setMethodInfo });
                }
            }
            else
            {
                Setter = CantWriteAction;
            }
        }
        
        public Func<TInstance, IValue> Getter { get; }

        public Action<TInstance, IValue> Setter { get; }

        public string Name { get; }

        public string Alias { get; }

        public bool CanRead => _propertyInfo.CanRead;
        public bool CanWrite => _propertyInfo.CanWrite;

        public BslPropertyInfo PropertyInfo => _propertyInfo;

        private Func<TInstance, IValue> CreateGetter<T>(MethodInfo methInfo)
        {
            var method = (Func<TInstance, T>)Delegate.CreateDelegate(typeof(Func<TInstance, T>), methInfo);
            return inst => ConvertReturnValue(method(inst));
        }

        private Action<TInstance, IValue> CreateSetter<T>(MethodInfo methInfo)
        {
            var method = (Action<TInstance, T>)Delegate.CreateDelegate(typeof(Action<TInstance, T>), methInfo);
            return (inst, val) => method(inst, ConvertParam<T>(val));
        }

        private T ConvertParam<T>(IValue value)
        {
            return ContextValuesMarshaller.ConvertParam<T>(value);
        }

        private IValue ConvertReturnValue<TRet>(TRet param)
        {
            return ContextValuesMarshaller.ConvertReturnValue(param);
        }

    }

    public class ContextPropertyMapper<TInstance>
    {
        private List<PropertyTarget<TInstance>> _properties;
        private readonly object _locker = new object();

        private void Init()
        {
            if (_properties != null) 
                return;

            lock (_locker)
            {
                if (_properties == null)
                {
                    _properties = FindProperties();
                }
            }
        }

        private List<PropertyTarget<TInstance>> FindProperties()
        {
            return typeof(TInstance).GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ContextPropertyAttribute), false).Any())
                .Select(x => new PropertyTarget<TInstance>(x)).ToList();
        }

        public bool ContainsProperty(string name)
        {
            return GetPropertyIndex(name) >= 0;
        }

        public int FindProperty(string name)
        {
            var idx = GetPropertyIndex(name);
            if (idx < 0)
                throw PropertyAccessException.PropNotFoundException(name);

            return idx;
        }

        public PropertyTarget<TInstance> GetProperty(int index)
        {
            Init();
            return _properties[index];
        }

        public int Count
        {
            get
            {
                Init();
                return _properties.Count;
            }
        }

        private int GetPropertyIndex(string name)
        {
            Init();
            return _properties.FindIndex(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) 
                || String.Equals(x.Alias, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
