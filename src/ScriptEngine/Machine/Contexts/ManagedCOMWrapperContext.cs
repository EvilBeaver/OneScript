/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    public class ManagedCOMWrapperContext : COMWrapperContext
    {
        private readonly Type _instanceType;
        private bool? _isIndexed;

        private readonly ComReflectionNameToIdMapper _nameMapper;

        public ManagedCOMWrapperContext(object instance) : base(instance)
        {
            _instanceType = instance.GetType();
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
                                        Instance,
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
            var result = pi.GetGetMethod().Invoke(Instance, null);
            return CreateIValue(result);
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            var pi = _nameMapper.GetProperty(propNum);
            
            var setMethod = pi.GetSetMethod();
            setMethod.Invoke(Instance, MarshalArgumentsStrict(new[] { newVal }, new[] { pi.PropertyType }));
        }

        public override IValue GetIndexedValue(IValue index)
        {
            if (!IsIndexed)
                throw RuntimeException.IndexedAccessIsNotSupportedException();

            var member = _instanceType.GetMethod("get_Item", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance);

            if (member == null) // set only?
                throw RuntimeException.IndexedAccessIsNotSupportedException();

            object retValue = member.Invoke(Instance, MarshalArgumentsStrict(new[] { index }, GetMethodParametersTypes(member)));

            return CreateIValue(retValue);

        }
        
        public override void SetIndexedValue(IValue index, IValue value)
        {
            if (!IsIndexed)
                throw RuntimeException.IndexedAccessIsNotSupportedException();

            var member = _instanceType.GetMethod("set_Item", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance);

            if (member == null) // get only?
                throw RuntimeException.IndexedAccessIsNotSupportedException();

            member.Invoke(Instance, MarshalArgumentsStrict(new[] { index, value }, GetMethodParametersTypes(member)));

        }

        public override int FindMethod(string name)
        {
            return _nameMapper.FindMethod(Instance, name);
        }

        public override BslMethodInfo GetMethodInfo(int methodNumber)
        {
            var methodInfo = _nameMapper.GetMethod(methodNumber).Method;
            return GetReflectableMethod(methodInfo);
        }

        public override VariableInfo GetPropertyInfo(int propertyNumber)
        {
            var info = _nameMapper.GetProperty(propertyNumber);
            return new VariableInfo
            {
                Identifier = info.Name,
                Index = propertyNumber,
                CanGet = info.CanRead,
                CanSet = info.CanWrite,
                Type = SymbolType.ContextProperty
            };
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

        private BslMethodInfo GetReflectableMethod(MethodInfo reflectionMethod)
        {
            var builder = BslMethodBuilder.Create();
            builder.Name(reflectionMethod.Name);
            builder.ReturnType(reflectionMethod.ReturnType);
            
            var reflectionParams = reflectionMethod.GetParameters();
            FillMethodInfoParameters(builder, reflectionParams);
            
            return builder.Build();
        }
        
        private static Type[] GetMethodParametersTypes(MethodInfo method)
        {
            return method.GetParameters()
                .Select(x => x.ParameterType)
                .ToArray();
        }

        private static void FillMethodInfoParameters(BslMethodBuilder<BslScriptMethodInfo> methodSignature, ParameterInfo[] reflectionParams)
        {
            foreach (var reflectedParam in reflectionParams)
            {
                var paramBuilder = methodSignature.NewParameter()
                    .ByValue(!reflectedParam.IsOut);

                if (!reflectedParam.HasDefaultValue) 
                    continue;
                
                var marshalled = ContextValuesMarshaller.ConvertReturnValue(reflectedParam.DefaultValue);
                Debug.Assert(marshalled is BslPrimitiveValue);
                paramBuilder.DefaultValue((BslPrimitiveValue)marshalled);
            }
        }
    }
}
//#endif