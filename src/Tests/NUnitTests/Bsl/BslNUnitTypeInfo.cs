/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using System.Linq;
using NUnit.Framework.Interfaces;

namespace NUnitTests.Bsl
{
    /// <summary>
    /// Реализация ITypeInfo для BSL-типов, используемая в интеграции с NUnit.
    /// </summary>
    public class BslNUnitTypeInfo : ITypeInfo
    {
        private readonly Type _type;

        public BslNUnitTypeInfo(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Type Type => _type;

        public ITypeInfo BaseType => _type.BaseType == null ? null : new BslNUnitTypeInfo(_type.BaseType);

        public Assembly Assembly => _type.Assembly;

        public bool ContainsGenericParameters => _type.ContainsGenericParameters;

        public string FullName => _type.FullName ?? _type.Name;

        public bool IsAbstract => _type.IsAbstract;

        public bool IsGenericParameter => _type.IsGenericParameter;

        public bool IsGenericType => _type.IsGenericType;

        public bool IsGenericTypeDefinition => _type.IsGenericTypeDefinition;

        public bool IsInterface => _type.IsInterface;

        public bool IsNested => _type.IsNested;

        public bool IsSealed => _type.IsSealed;

        public bool IsStaticClass => _type.IsAbstract && _type.IsSealed;

        public string Name => _type.Name;

        public string Namespace => _type.Namespace;

        public Type[] GetGenericArguments()
        {
            return _type.GetGenericArguments();
        }

        public bool IsType(Type type)
        {
            return _type == type;
        }

        public string GetDisplayName()
        {
            return _type.FullName ?? _type.Name;
        }

        public string GetDisplayName(object[] args)
        {
            if (args == null || args.Length == 0)
                return GetDisplayName();

            var formattedArgs = string.Join(", ", args.Select(arg => arg?.ToString() ?? "null"));
            return $"{GetDisplayName()}({formattedArgs})";
        }

        public Type GetGenericTypeDefinition()
        {
            if (!_type.IsGenericType)
                throw new InvalidOperationException("Type is not a generic type");
            
            return _type.GetGenericTypeDefinition();
        }

        public IMethodInfo[] GetMethods(BindingFlags flags)
        {
            // TODO: Реализовать получение методов BSL-типа
            throw new NotImplementedException("TODO: Реализовать получение методов BSL-типа через BSL-движок");
        }

        public ITypeInfo MakeGenericType(Type[] typeArguments)
        {
            if (!_type.IsGenericTypeDefinition)
                throw new InvalidOperationException("Type is not a generic type definition");
            
            return new BslNUnitTypeInfo(_type.MakeGenericType(typeArguments));
        }

        public bool HasMethodWithAttribute(Type attrType)
        {
            return _type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Any(method => method.IsDefined(attrType, true));
        }

        public ConstructorInfo GetConstructor(Type[] argTypes)
        {
            return _type.GetConstructor(argTypes);
        }

        public bool HasConstructor(Type[] argTypes)
        {
            return GetConstructor(argTypes) != null;
        }

        public object Construct(object[] args)
        {
            // TODO: вызвать конструктор через BSL-движок, если требуется
            return Activator.CreateInstance(_type, args ?? Array.Empty<object>());
        }

        public IMethodInfo[] GetMethodsWithAttribute<T>(bool inherit) where T : class
        {
            // TODO: Реализовать получение методов по атрибутам через BSL-движок
            throw new NotImplementedException("TODO: Реализовать получение методов по атрибутам через BSL-движок");
        }

        public bool IsDefined<T>(bool inherit) where T : class
        {
            return _type.IsDefined(typeof(T), inherit);
        }

        public T[] GetCustomAttributes<T>(bool inherit) where T : class
        {
            var attrs = _type.GetCustomAttributes(typeof(T), inherit);
            var result = new T[attrs.Length];
            for (int i = 0; i < attrs.Length; i++)
            {
                result[i] = (T)attrs[i];
            }

            return result;
        }
    }
}

