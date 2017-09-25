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
using System.Text;

namespace ScriptEngine.Machine
{
    class ComReflectionNameToIdMapper
    {
        private readonly Type _reflectedType;
        private readonly IndexedNamesCollection _propertyNames;
        private readonly IndexedNamesCollection _methodNames;
        private readonly List<PropertyInfo> _propertyCache;
        private readonly List<Func<IValue[], object>> _methodsCache;

        public ComReflectionNameToIdMapper(Type type)
        {
            _reflectedType = type;
            _propertyNames = new IndexedNamesCollection();
            _methodNames = new IndexedNamesCollection();
            _propertyCache = new List<PropertyInfo>();
            _methodsCache = new List<Func<IValue[], object>>();
        }

        public int FindProperty(string name)
        {
            int id;
            var hasProperty = _propertyNames.TryGetIdOfName(name, out id);
            if(!hasProperty)
            {
                var propInfo = _reflectedType.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (propInfo == null)
                    throw RuntimeException.PropNotFoundException(name);

                id = _propertyNames.RegisterName(name);
                System.Diagnostics.Debug.Assert(_propertyCache.Count == id);
                
                _propertyCache.Add(propInfo);
                
            }

            return id;
        }

        private System.Reflection.MethodInfo[] GetMethods(string name)
        {
            return _reflectedType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                .Where((x) => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        }

        private static System.Reflection.MethodInfo ChooseBestMatchingMethod(System.Reflection.MethodInfo[] methods, IValue[] callParams)
        {
            // TODO: анализ типов, анализ параметров по-умолчанию
            foreach (var mi in methods)
            {
                if (mi.GetParameters().Length == callParams.Length)
                {
                    return mi;
                }
            }
            return methods[0];
        }

        private static object InvokeMethod(object instance, System.Reflection.MethodInfo[] methods, IValue[] callParams)
        {
            var mi = ChooseBestMatchingMethod(methods, callParams);
            object[] castedParams = Contexts.COMWrapperContext.MarshalArgumentsStrict(mi, callParams);
            return mi.Invoke(instance, castedParams);
        }

        public delegate object InvokationDelegate(IValue[] callParams);

        public int FindMethod(object instance, string name)
        {
            int id;
            var hasMethod = _methodNames.TryGetIdOfName(name, out id);
            if (!hasMethod)
            {
                Func< IValue[], object > invoker = (IValue[] callParams) =>
                {
                    return instance.GetType().InvokeMember(name,
                        BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
                            | BindingFlags.Public | BindingFlags.OptionalParamBinding
                            | BindingFlags.Instance,
                        new ValueBinder(),
                        instance,
                        callParams.Cast<object>().ToArray());
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
