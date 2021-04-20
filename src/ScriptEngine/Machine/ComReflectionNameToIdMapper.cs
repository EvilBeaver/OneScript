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

namespace ScriptEngine.Machine
{
    class ComReflectionNameToIdMapper
    {
        private readonly Type _reflectedType;
        private readonly IndexedNamesCollection _propertyNames;
        private readonly IndexedNamesCollection _methodNames;
        private List<PropertyInfo> _propertyCache;
        private readonly List<Func<IValue[], object>> _methodsCache;

        public ComReflectionNameToIdMapper(Type type)
        {
            _reflectedType = type;
            _propertyNames = new IndexedNamesCollection();
            _methodNames = new IndexedNamesCollection();
            _propertyCache = null;
            _methodsCache = new List<Func<IValue[], object>>();
        }

        public IReadOnlyList<PropertyInfo> GetProperties()
        {
            GetAllProperties();
            return _propertyCache;
        }
        
        private void GetAllProperties()
        {
            if(_propertyCache != null)
                return;
            
            _propertyCache = new List<PropertyInfo>();
            
            var allProps = _reflectedType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var propInfo in allProps)
            {
                _propertyNames.RegisterName(propInfo.Name);
                _propertyCache.Add(propInfo);
            }
        }
        
        public int FindProperty(string name)
        {
            GetAllProperties();
            var hasProperty = _propertyNames.TryGetIdOfName(name, out var id);
            if (!hasProperty)
                throw OldRuntimeException.PropNotFoundException(name);

            return id;
        }

        public int FindMethod(object instance, string name)
        {
            int id;
            var hasMethod = _methodNames.TryGetIdOfName(name, out id);
            if (!hasMethod)
            {
                Func< IValue[], object > invoker = (IValue[] callParams) =>
                {

                    try
                    {
                        return instance.GetType().InvokeMember(name,
                                        BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
                                            | BindingFlags.Public | BindingFlags.OptionalParamBinding
                                            | BindingFlags.Instance,
                                        new ValueBinder(),
                                        instance,
                                        callParams.Cast<object>().ToArray());
                    }
                    catch (TargetInvocationException e)
                    {
                        if (e.InnerException != null)
                            throw e.InnerException;

                        throw;
                    }
                };

                id = _methodNames.RegisterName(name);
                System.Diagnostics.Debug.Assert(_methodsCache.Count == id);

                _methodsCache.Add(invoker);
            }

            return id;
        }

        public PropertyInfo GetProperty(int id)
        {
            return _propertyCache[id];
        }

        public Func<IValue[], object> GetMethod(int id)
        {
            return _methodsCache[id];
        }
    }
}
