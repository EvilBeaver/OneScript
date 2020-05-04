/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ScriptEngine.Machine.Contexts
{
    class ManagedCOMWrapperContext : COMWrapperContext
    {
        private readonly Type _instanceType;
        private readonly object _instance;
        private bool? _isIndexed;

        private readonly ComReflectionNameToIdMapper _nameMapper;

        public ManagedCOMWrapperContext(object instance)
        {
            _instanceType = instance.GetType();
            _instance = instance;
            _nameMapper = new ComReflectionNameToIdMapper(_instanceType);
        }

        public override int GetPropCount()
        {
            return _nameMapper.GetProperties().Count;
        }

        public override string GetPropName(int propNum)
        {
            var prop = _nameMapper.GetProperty(propNum);
            return prop.Name;
        }

        public override bool IsIndexed
        {
            get
            {
                if (_isIndexed == null)
                {
                    _isIndexed = _instanceType.GetProperties().Any(x => x.GetIndexParameters().Length > 0);
                }     
                
                return (bool)_isIndexed;
            }
        }

        public override IEnumerator<IValue> GetEnumerator()
        {
            System.Collections.IEnumerator comEnumerator;

            try
            {

                comEnumerator = (System.Collections.IEnumerator)_instanceType.InvokeMember("GetEnumerator",
                                        BindingFlags.InvokeMethod,
                                        null,
                                        _instance,
                                        new object[0]);
            }
            catch (MissingMethodException)
            {
                throw RuntimeException.IteratorIsNotDefined();
            }

            while (comEnumerator.MoveNext())
            {
                yield return CreateIValue(comEnumerator.Current);
            }
        }

        public override object UnderlyingObject
        {
            get { return _instance; }
        }

        public override int FindProperty(string name)
        {
            return _nameMapper.FindProperty(name);
        }

        public override bool IsPropReadable(int propNum)
        {
            PropertyInfo pi = _nameMapper.GetProperty(propNum);
            return pi.CanRead;
        }

        public override bool IsPropWritable(int propNum)
        {
            PropertyInfo pi = _nameMapper.GetProperty(propNum);
            return pi.CanWrite;
        }

        public override IValue GetPropValue(int propNum)
        {
            var pi = _nameMapper.GetProperty(propNum);
            var result = pi.GetGetMethod().Invoke(_instance, null);
            return CreateIValue(result);
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            var pi = _nameMapper.GetProperty(propNum);
            
            var setMethod = pi.GetSetMethod();
            setMethod.Invoke(_instance, MarshalArgumentsStrict(new[] { newVal }, new[] { pi.PropertyType }));
        }

        public override IValue GetIndexedValue(IValue index)
        {
            if (!IsIndexed)
                throw RuntimeException.IndexedAccessIsNotSupportedException();

            var member = _instanceType.GetMethod("get_Item", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance);

            if (member == null) // set only?
                throw RuntimeException.IndexedAccessIsNotSupportedException();

            object retValue = member.Invoke(_instance, MarshalArgumentsStrict(new[] { index }, GetMethodParametersTypes(member)));

            return CreateIValue(retValue);

        }
        
        public override void SetIndexedValue(IValue index, IValue value)
        {
            if (!IsIndexed)
                throw RuntimeException.IndexedAccessIsNotSupportedException();

            var member = _instanceType.GetMethod("set_Item", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance);

            if (member == null) // get only?
                throw RuntimeException.IndexedAccessIsNotSupportedException();

            object retValue = member.Invoke(_instance, MarshalArgumentsStrict(new[] { index, value }, GetMethodParametersTypes(member)));

        }

        public override int FindMethod(string name)
        {
            return _nameMapper.FindMethod(_instance, name);
        }

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            var methodInfo = _nameMapper.GetMethod(methodNumber);
            return GetReflectableMethod(methodInfo.Method);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            var method = _nameMapper.GetMethod(methodNumber);
            method.Invoke(arguments);
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            var method = _nameMapper.GetMethod(methodNumber);
            var result = method.Invoke(arguments);
            retValue = CreateIValue(result);
        }

        private MethodInfo GetReflectableMethod(System.Reflection.MethodInfo reflectionMethod)
        {
            MethodInfo methodInfo;

            methodInfo = new MethodInfo();
            methodInfo.Name = reflectionMethod.Name;

            var reflectedMethod = reflectionMethod as System.Reflection.MethodInfo;

            if (reflectedMethod != null)
            {
                methodInfo.IsFunction = reflectedMethod.ReturnType != typeof(void);
                var reflectionParams = reflectedMethod.GetParameters();
                FillMethodInfoParameters(ref methodInfo, reflectionParams);
            }

            return methodInfo;
        }
        
        private static Type[] GetMethodParametersTypes(System.Reflection.MethodInfo method)
        {
            return method.GetParameters()
                .Select(x => x.ParameterType)
                .ToArray();
        }

        private static void FillMethodInfoParameters(ref MethodInfo methodInfo, System.Reflection.ParameterInfo[] reflectionParams)
        {
            methodInfo.Params = new ParameterDefinition[reflectionParams.Length];
            for (int i = 0; i < reflectionParams.Length; i++)
            {
                var reflectedParam = reflectionParams[i];
                var param = new ParameterDefinition();
                param.HasDefaultValue = reflectedParam.IsOptional;
                param.IsByValue = !reflectedParam.IsOut;
                methodInfo.Params[i] = param;
            }
        }
    }
}
//#endif