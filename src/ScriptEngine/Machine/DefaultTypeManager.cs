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
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace ScriptEngine.Machine
{
    public class DefaultTypeManager : ITypeManager
    {
        // Типы регистрируются и из фоновых заданий: ПодключитьСценарий, внешние компоненты.
        // Поиск по имени идет на каждом Новый и Тип(), поэтому он без блокировки,
        // а регистрация и редкие обращения к списку типов - под _lock.
        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<string, TypeDescriptor> _knownTypesByName = new ConcurrentDictionary<string, TypeDescriptor>(StringComparer.InvariantCultureIgnoreCase);
        private readonly List<TypeDescriptor> _knownTypes = new List<TypeDescriptor>();
        private readonly TypeFactoryCache _factoryCache = new TypeFactoryCache();
        private readonly ILazyTypeResolver[] _resolvers;

        public DefaultTypeManager(IEnumerable<ILazyTypeResolver> resolvers = null)
        {
            _resolvers = resolvers?.ToArray() ?? Array.Empty<ILazyTypeResolver>();

            RegisterTypeInternal(BasicTypes.Undefined);
            RegisterTypeInternal(BasicTypes.Boolean);
            RegisterTypeInternal(BasicTypes.String);
            RegisterTypeInternal(BasicTypes.Date);
            RegisterTypeInternal(BasicTypes.Number);
            RegisterTypeInternal(BasicTypes.Null);
            RegisterTypeInternal(BasicTypes.Type);
            
            // TODO тут был еще тип Object для конструирования
        }

        #region ITypeManager Members

        public TypeDescriptor GetTypeByName(string name)
        {
            if (_knownTypesByName.TryGetValue(name, out var knownType))
            {
                return knownType;
            }

            if (TryResolveLazily(name, out var resolvedType))
            {
                return resolvedType;
            }

            var clrType = Type.GetType(name, throwOnError: false, ignoreCase: true);
            if (clrType != null)
            {
                var td = RegisterType(name, default, typeof(COMWrapperContext));
                return td;
            }
          
            throw RuntimeException.TypeIsNotRegistered(name);
        }

        public bool TryGetType(Type frameworkType, out TypeDescriptor type)
        {
            lock (_lock)
            {
                type = _knownTypes.FirstOrDefault(x => x.ImplementingClass == frameworkType);
            }

            return type != default;
        }
        
        public bool TryGetType(string name, out TypeDescriptor type)
        {
            if (_knownTypesByName.TryGetValue(name, out type))
            {
                return true;
            }

            if (TryResolveLazily(name, out type))
            {
                return true;
            }

            type = default;
            return false;
        }

        public TypeDescriptor RegisterType(string name, string alias, Type implementingClass)
        {
            lock (_lock)
            {
                if (_knownTypesByName.TryGetValue(name, out var td))
                {
                    if (td.ImplementingClass != implementingClass)
                    {
                        throw new InvalidOperationException($"Name `{name}` is already registered");
                    }

                    return td;
                }

                var typeDesc = new TypeDescriptor(implementingClass, name, alias);
                RegisterTypeInternal(typeDesc);
                return typeDesc;
            }
        }
        
        public void RegisterType(TypeDescriptor typeDescriptor)
        {
            lock (_lock)
            {
                if (_knownTypesByName.TryGetValue(typeDescriptor.Name, out var knownType))
                {
                    if (knownType != typeDescriptor)
                        throw new InvalidOperationException($"Type {typeDescriptor} already registered");

                    return;
                }

                RegisterTypeInternal(typeDescriptor);
            }
        }

        public ITypeFactory GetFactoryFor(TypeDescriptor type)
        {
            return _factoryCache.GetFactoryFor(type);
        }

        private void RegisterTypeInternal(TypeDescriptor td)
        {
            _knownTypes.Add(td);
            _knownTypesByName[td.Name] = td;
            if (!string.IsNullOrWhiteSpace(td.Alias) && td.Alias != td.Name)
                _knownTypesByName[td.Alias] = td;
        }

        private bool TryResolveLazily(string name, out TypeDescriptor type)
        {
            foreach (var resolver in _resolvers)
            {
                if (resolver.TryResolve(name, this, out type))
                {
                    return true;
                }
            }

            type = default;
            return false;
        }

        public TypeDescriptor GetTypeByFrameworkType(Type type)
        {
            lock (_lock)
            {
                return _knownTypes.First(x => x.ImplementingClass == type);
            }
        }

        public bool IsKnownType(Type type)
        {
            lock (_lock)
            {
                return _knownTypes.Any(x => x.ImplementingClass == type);
            }
        }

        public bool IsKnownType(string typeName)
        {
            var nameToUpper = typeName.ToUpperInvariant();
            lock (_lock)
            {
                return _knownTypes.Any(x => x.Name.ToUpperInvariant() == nameToUpper);
            }
        }

        public IReadOnlyList<TypeDescriptor> RegisteredTypes()
        {
            // Копия: вызывающий перебирает список уже без блокировки
            lock (_lock)
            {
                return _knownTypes.ToArray();
            }
        }

        #endregion

    }
}
