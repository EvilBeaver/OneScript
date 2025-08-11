/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;
using TinyIoC;

namespace ScriptEngine.Machine
{
    public class NotBslValueWrapper : ContextIValueImpl, IObjectWrapper
    {
        private readonly object _initLocker = new object();
        private readonly Type _underlyingType;
        
        private static readonly Dictionary<Type, IndexedNamesCollection> PropertiesIndexers = 
            new Dictionary<Type, IndexedNamesCollection>();
        private static readonly Dictionary<Type, IndexedNamesCollection> MethodsIndexers = 
            new Dictionary<Type, IndexedNamesCollection>();

        private static readonly Dictionary<Type, Dictionary<int, ContextPropertyInfo>> PropertiesCaches =
            new Dictionary<Type, Dictionary<int, ContextPropertyInfo>>();
        private static readonly Dictionary<Type, Dictionary<int, ContextMethodInfo>> MethodsCaches =
            new Dictionary<Type, Dictionary<int, ContextMethodInfo>>();

        public object UnderlyingObject { get; }

        public NotBslValueWrapper(object obj)
        {
            UnderlyingObject = obj;
            _underlyingType = UnderlyingObject.GetType();
            
            DefineType(obj.GetType().GetTypeFromClassMarkup());
            InitMethodsProperties();
        }

        private void InitMethodsProperties()
        {
            var objType = UnderlyingObject.GetType();

            lock (_initLocker)
            {
                // Свойства и методы уже были закешированы
                if (PropertiesIndexers.ContainsKey(objType))
                    return;
                
                var propertiesIndex = new IndexedNamesCollection();
                var propertiesCache = new Dictionary<int, ContextPropertyInfo>();
                
                var methodsIndex = new IndexedNamesCollection();
                var methodsCache = new Dictionary<int, ContextMethodInfo>();
                
                
                var props = objType.GetProperties()
                    .Where(x => x.GetCustomAttributes(typeof(ContextPropertyAttribute), false).Length != 0)
                    .ToList();

                foreach (var property in props)
                {
                    var info = new ContextPropertyInfo(property);
                    var id = propertiesIndex.RegisterName(info.Name, info.Alias);
                    
                    propertiesCache.Add(id, info);
                }

                var methods = objType.GetMethods()
                    .Where(x => x.GetCustomAttributes(typeof(ContextMethodAttribute), false).Length != 0)
                    .ToList();
                
                foreach (var method in methods)
                {
                    var info = new ContextMethodInfo(method);
                    var id = methodsIndex.RegisterName(info.Name, info.Alias);
                    
                    methodsCache.Add(id, info);
                }
                
                PropertiesIndexers.Add(objType, propertiesIndex);
                PropertiesCaches.Add(objType, propertiesCache);
                
                MethodsIndexers.Add(objType, methodsIndex);
                MethodsCaches.Add(objType, methodsCache);
            }
        }

        public override int GetPropertyNumber(string name)
            => GetPropertiesIndexer().TryGetIdOfName(name, out var id) ? id : -1;

        public override bool IsPropReadable(int propNum)
            => GetPropertiesCache()[propNum].CanRead;

        public override bool IsPropWritable(int propNum)
            => GetPropertiesCache()[propNum].CanWrite;

        public override IValue GetPropValue(int propNum)
        {
            var prop = GetPropertiesCache()[propNum];
            var getter = prop.GetGetMethod(true);
            if (getter == null)
                    throw PropertyAccessException.PropIsNotReadableException(prop.Name);
            var value = getter.Invoke(UnderlyingObject, Array.Empty<object>());

            if (!prop.TryGetConverter(out var converter)) 
                return ContextValuesMarshaller.ConvertDynamicValue(value);
            
            var toIValue = converter.GetType().GetMethod("ToIValue");
            return (IValue)toIValue!.Invoke(converter, new[] { value });
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            var prop = GetPropertiesCache()[propNum];
            
            var setter = prop.GetSetMethod(true);
            if (setter == null) 
                throw PropertyAccessException.PropIsNotWritableException(prop.Name);

            object val;
            
            if (prop.TryGetConverter(out var converter))
            {
                var type = converter.GetType();
                var toClrMethod = type
                    .GetMethod("ToClr", new [] { typeof(IValue) });
                
                val = toClrMethod!.Invoke(converter, new object[] { newVal });
            }
            else
                val = ContextValuesMarshaller.ConvertToClrObject(newVal);
            
            var propType = prop.PropertyType;
            if (val is NotBslValueWrapper wrapper && propType.IsInstanceOfType(wrapper.UnderlyingObject))
                val = wrapper.UnderlyingObject;
            
            setter.Invoke(UnderlyingObject, new [] { val });
        }

        public override int GetPropCount()
            => GetPropertiesCache().Count;

        public override string GetPropName(int propNum)
            => GetPropertiesCache()[propNum].Name;

        public override int GetMethodNumber(string name)
            => GetMethodsIndexer().TryGetIdOfName(name, out var id) ? id : -1;

        public override int GetMethodsCount()
            => GetMethodsCache().Count;

        public override BslMethodInfo GetMethodInfo(int methodNumber)
            => GetMethodsCache()[methodNumber];

        public override BslPropertyInfo GetPropertyInfo(int propertyNumber)
            => GetPropertiesCache()[propertyNumber];

        public override void CallAsProcedure(int methodNumber, IValue[] arguments, IBslProcess process)
        {
            var method = GetMethodsCache()[methodNumber];
            
            var args = arguments.Select(ContextValuesMarshaller.ConvertToClrObject).ToArray();
            method.Invoke(UnderlyingObject, args);
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue, IBslProcess process)
        {
            var method = GetMethodsCache()[methodNumber];
            
            var args = arguments.Select(c => ContextValuesMarshaller.ConvertParam(c, method.ReturnType, process)).ToArray();
            var result = method.Invoke(UnderlyingObject, args);
            
            if (method.TryGetConverter(out var converter))
            {
                var type = converter.GetType();
                var convertMethod = type
                    .GetMethod("ToIValue", new [] { method.ReturnType });
                
                retValue = (IValue)convertMethod!.Invoke(converter, new[] { result });
            }
            else
                retValue = ContextValuesMarshaller.ConvertDynamicValue(result);
        }

        private static int GetMemberNumberByName<T>(Dictionary<int, T> items, string name)
            where T : INameAndAliasProvider
        {
            foreach (var kvp in from kvp in items
                     let v = kvp.Value
                     where string.Equals(v.Name, name, StringComparison.InvariantCultureIgnoreCase) ||
                           string.Equals(v.Alias, name, StringComparison.InvariantCultureIgnoreCase)
                     select kvp)
            {
                return kvp.Key;
            }

            return -1;
        }
        
        private IndexedNamesCollection GetMethodsIndexer()
            => MethodsIndexers[_underlyingType];
        
        private IndexedNamesCollection GetPropertiesIndexer()
            => PropertiesIndexers[_underlyingType];

        private Dictionary<int, ContextMethodInfo> GetMethodsCache()
            => MethodsCaches[_underlyingType];
        
        private Dictionary<int, ContextPropertyInfo> GetPropertiesCache()
            => PropertiesCaches[_underlyingType];
    }
}
