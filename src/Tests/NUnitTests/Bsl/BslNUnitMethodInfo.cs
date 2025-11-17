/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Interfaces;
using ScriptEngine.Machine.Contexts;

namespace NUnitTests.Bsl
{
    /// <summary>
    /// Реализация IMethodInfo для BSL-методов, используемая в интеграции с NUnit.
    /// </summary>
    public class BslNUnitMethodInfo : IMethodInfo
    {
        private readonly InvokableBslMethodInfo _methodInfo;
        private readonly UserScriptContextInstance _testInstance;

        public BslNUnitMethodInfo(InvokableBslMethodInfo methodInfo, UserScriptContextInstance testInstance)
        {
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            _testInstance = testInstance ?? throw new ArgumentNullException(nameof(testInstance));
        }

        public ITypeInfo TypeInfo => new BslNUnitTypeInfo(_methodInfo.DeclaringType);

        public MethodInfo MethodInfo => _methodInfo;

        public string Name => _methodInfo.Name;

        public bool IsAbstract => _methodInfo.IsAbstract;

        public bool IsPublic => _methodInfo.IsPublic;

        public bool IsStatic => _methodInfo.IsStatic;

        public bool ContainsGenericParameters => _methodInfo.ContainsGenericParameters;

        public bool IsGenericMethod => _methodInfo.IsGenericMethod;

        public bool IsGenericMethodDefinition => _methodInfo.IsGenericMethodDefinition;

        public ITypeInfo ReturnType => new BslNUnitTypeInfo(_methodInfo.ReturnType);

        public IParameterInfo[] GetParameters()
        {
            return _methodInfo.GetParameters()
                .Select(p => new BslNUnitParameterInfo(this, p))
                .Cast<IParameterInfo>()
                .ToArray();
        }

        public Type[] GetGenericArguments()
        {
            return _methodInfo.GetGenericArguments();
        }

        public IMethodInfo MakeGenericMethod(params Type[] typeArguments)
        {
            var genericMethod = _methodInfo.MakeGenericMethod(typeArguments);
            return new BslNUnitMethodInfo(
                genericMethod as InvokableBslMethodInfo ?? throw new InvalidOperationException("MakeGenericMethod returned unexpected type"),
                _testInstance);
        }

        public object Invoke(object fixture, params object[] args)
        {
            // TODO: Реализовать вызов BSL-метода через движок BSL
            // NUnit будет вызывать этот метод для выполнения теста
            // Нужно вызвать метод на объекте _testInstance с параметрами args
            return _methodInfo.Invoke(fixture ?? _testInstance, args ?? Array.Empty<object>());
        }

        public bool IsDefined<T>(bool inherit) where T : class
        {
            return _methodInfo.IsDefined(typeof(T), inherit);
        }

        public T[] GetCustomAttributes<T>(bool inherit) where T : class
        {
            var attrs = _methodInfo.GetCustomAttributes(typeof(T), inherit);
            var result = new T[attrs.Length];
            for (int i = 0; i < attrs.Length; i++)
            {
                result[i] = (T)attrs[i];
            }
            return result;
        }
    }
}

