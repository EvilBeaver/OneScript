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
        private readonly IndexedNamesCollection _propertiesCache = new IndexedNamesCollection();
        private readonly Dictionary<int, ContextPropertyInfo> _properties = new Dictionary<int, ContextPropertyInfo>();

        private readonly IndexedNamesCollection _methodsCache = new IndexedNamesCollection();
        private readonly Dictionary<int, ContextMethodInfo> _methods = new Dictionary<int, ContextMethodInfo>();

        public object UnderlyingObject { get; }

        public NotBslValueWrapper(object obj)
        {
            UnderlyingObject = obj;
            DefineType(obj.GetType().GetTypeFromClassMarkup());

            InitMethodsProperties();
        }

        private void InitMethodsProperties()
        {
            var objType = UnderlyingObject.GetType();

            var props = objType.GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ContextPropertyAttribute), false).Length != 0)
                .ToList();

            for (var i = 0; i < props.Count; i++)
            {
                var contextInfo = new ContextPropertyInfo(props[i]);
                
                _properties.Add(i, contextInfo);
                _propertiesCache.RegisterName(contextInfo.Name, contextInfo.Alias);
            }

            var methods = objType.GetMethods()
                .Where(x => x.GetCustomAttributes(typeof(ContextMethodAttribute), false).Length != 0)
                .ToList();

            for (var i = 0; i < methods.Count; i++)
            {
                var contextInfo = new ContextMethodInfo(methods[i]);
                
                _methods.Add(i, contextInfo);
                _methodsCache.RegisterName(contextInfo.Name, contextInfo.Alias);
            }
        }

        public override int GetPropertyNumber(string name)
            => _propertiesCache.TryGetIdOfName(name, out var id) ? id : GetMemberNumberByName(_properties, name);

        public override bool IsPropReadable(int propNum)
            => _properties[propNum].CanRead;

        public override bool IsPropWritable(int propNum)
            => _properties[propNum].CanWrite;

        public override IValue GetPropValue(int propNum)
        {
            var prop = _properties[propNum];
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
            var prop = _properties[propNum];
            
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
            if (val is NotBslValueWrapper wrapper && wrapper.UnderlyingObject.GetType() == propType)
                val = wrapper.UnderlyingObject;
            
            setter.Invoke(UnderlyingObject, new [] { val });
        }

        public override int GetPropCount()
            => _properties.Count;

        public override string GetPropName(int propNum)
            => _properties[propNum].Name;

        public override int GetMethodNumber(string name)
            => _methodsCache.TryGetIdOfName(name, out var id) ? id : GetMemberNumberByName(_methods, name);

        public override int GetMethodsCount()
            => _methods.Count;

        public override BslMethodInfo GetMethodInfo(int methodNumber)
            => _methods[methodNumber];

        public override BslPropertyInfo GetPropertyInfo(int propertyNumber)
            => _properties[propertyNumber];

        public override void CallAsProcedure(int methodNumber, IValue[] arguments, IBslProcess process)
        {
            var method = _methods[methodNumber];
            
            var args = arguments.Select(ContextValuesMarshaller.ConvertToClrObject).ToArray();
            method.Invoke(UnderlyingObject, args);
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue, IBslProcess process)
        {
            var method = _methods[methodNumber];
            
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
            foreach (var kvp in items.Where(kvp => string.Equals(kvp.Value.Name,  name, StringComparison.InvariantCultureIgnoreCase) ||
                                                   string.Equals(kvp.Value.Alias, name, StringComparison.InvariantCultureIgnoreCase)))
                return kvp.Key;

            return -1;
        }
    }
}
