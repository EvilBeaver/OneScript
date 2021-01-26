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
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Values;
using ScriptEngine.Types;

namespace ScriptEngine.Machine
{
    public class DefaultTypeManager : ITypeManager
    {
        private readonly Dictionary<string, int> _knownTypesIndexes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private readonly List<TypeDescriptor> _knownTypes = new List<TypeDescriptor>();
        private readonly TypeActivator _activator = new TypeActivator();
        
        private Type _dynamicFactory;

        public DefaultTypeManager()
        {
            RegisterType(BasicTypes.Undefined);
            RegisterType(BasicTypes.Boolean);
            RegisterType(BasicTypes.String);
            RegisterType(BasicTypes.Date);
            RegisterType(BasicTypes.Number);
            RegisterType(BasicTypes.Null);
            RegisterType(BasicTypes.Type);
            
            // TODO тут был еще тип Object для конструирования
        }

        #region ITypeManager Members

        public TypeDescriptor GetTypeByName(string name)
        {
            if (_knownTypesIndexes.TryGetValue(name, out var index))
            {
                return _knownTypes[index];
            }
            var clrType = Type.GetType(name, throwOnError: false, ignoreCase: true);
            if (clrType != null)
            {
                var td = RegisterType(name, default, typeof(COMWrapperContext));
                return td;
            }
            throw new RuntimeException($"Тип не зарегистрирован ({name})");
        }

        public TypeDescriptor RegisterType(string name, string alias, Type implementingClass)
        {
            if (_knownTypesIndexes.ContainsKey(name))
            {
                var td = GetTypeByName(name);
                if (td.ImplementingClass != implementingClass)
                {
                    throw new InvalidOperationException($"Name `{name}` is already registered");
                }

                return td;
            }
            else
            {
                var typeDesc = new TypeDescriptor(Guid.NewGuid(), name, alias, implementingClass);
                RegisterType(typeDesc);
                return typeDesc;
            }

        }

        public TypeFactory GetFactoryFor(TypeDescriptor type)
        {
            return _activator.GetFactoryFor(type);
        }

        private void RegisterType(TypeDescriptor td)
        {
            var nextListId = _knownTypes.Count;
            _knownTypesIndexes.Add(td.Name, nextListId);
            if (!string.IsNullOrWhiteSpace(td.Alias) && td.Alias != td.Name)
                _knownTypesIndexes[td.Alias] = nextListId;
            
            _knownTypes.Add(td);
        }

        public TypeDescriptor GetTypeByFrameworkType(Type type)
        {
            return _knownTypes.First(x => x.ImplementingClass == type);
        }

        public bool IsKnownType(Type type)
        {
            return _knownTypes.Any(x => x.ImplementingClass == type);
        }

        public bool IsKnownType(string typeName)
        {
            var nameToUpper = typeName.ToUpperInvariant();
            return _knownTypes.Any(x => x.Name.ToUpperInvariant() == nameToUpper);
        }

        public Type NewInstanceHandler 
        { 
            get
            {
                return _dynamicFactory;
            }

            set
            {
                if (value
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Any(x => x.GetCustomAttributes(false).Any<object>(y => y is ScriptConstructorAttribute)))
                {
                    _dynamicFactory = value;
                }
                else
                {
                    throw new InvalidOperationException("Class " + value.ToString() + " can't be registered as New handler");
                }
            }
        }

        #endregion

    }
}