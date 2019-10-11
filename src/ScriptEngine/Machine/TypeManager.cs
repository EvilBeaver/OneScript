/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    public interface ITypeManager
    {
        Type GetImplementingClass(int typeId);
        TypeDescriptor GetTypeByName(string name);
        TypeDescriptor GetTypeById(int id);
        TypeDescriptor GetTypeByFrameworkType(Type type);
        TypeDescriptor RegisterType(string name, Type implementingClass);
        TypeDescriptor GetTypeDescriptorFor(IValue typeTypeValue);
        void RegisterAliasFor(TypeDescriptor td, string alias);
        bool IsKnownType(Type type);
        bool IsKnownType(string typeName);
        Type NewInstanceHandler { get; set; }
    }

    class StandartTypeManager : ITypeManager
    {
        private readonly Dictionary<string, int> _knownTypesIndexes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private readonly List<KnownType> _knownTypes = new List<KnownType>();
        private Type _dynamicFactory;

        private struct KnownType
        {
            public Type SystemType;
            public TypeDescriptor Descriptor;
        }

        public StandartTypeManager()
        {
            foreach (var item in Enum.GetValues(typeof(DataType)))
            {
                DataType typeEnum = (DataType)item;
                string name;
                string alias;
                switch (typeEnum)
                {
                    case DataType.Undefined:
                        name = "Неопределено";
                        alias = "Undefined";
                        break;
                    case DataType.Boolean:
                        name = "Булево";
                        alias = "Boolean";
                        break;
                    case DataType.String:
                        name = "Строка";
                        alias = "String";
                        break;
                    case DataType.Date:
                        name = "Дата";
                        alias = "Date";
                        break;
                    case DataType.Number:
                        name = "Число";
                        alias = "Number";
                        break;
                    case DataType.Type:
                        name = "Тип";
                        alias = "Type";
                        break;
                    case DataType.Object:
                        name = "Object";
                        alias = null;
                        break;
                    default:
                        continue;
                }

                var td = new TypeDescriptor()
                {
                    Name = name,
                    ID = (int)typeEnum
                };

                RegisterType(td, typeof(DataType));

                if (alias != null)
                {
                    RegisterAliasFor(td, alias);
                }

            }

            RegisterType("Null", typeof(NullValueImpl));
        }

        #region ITypeManager Members

        public Type GetImplementingClass(int typeId)
        {
            var kt = _knownTypes.First(x => x.Descriptor.ID == typeId);
            return kt.SystemType;
        }

        public TypeDescriptor GetTypeByName(string name)
        {
            if (_knownTypesIndexes.ContainsKey(name))
            {
                return _knownTypes[_knownTypesIndexes[name]].Descriptor;
            }
            var clrType = Type.GetType(name, throwOnError: false, ignoreCase: true);
            if (clrType != null)
            {
                var td = RegisterType(name, typeof(COMWrapperContext));
                return td;
            }
            throw new RuntimeException(String.Format("Тип не зарегистрирован ({0})", name));
        }

        public TypeDescriptor GetTypeById(int id)
        {
            return _knownTypes[id].Descriptor;
        }

        public TypeDescriptor RegisterType(string name, Type implementingClass)
        {
            if (_knownTypesIndexes.ContainsKey(name))
            {
                var td = GetTypeByName(name);
				if (GetImplementingClass(td.ID) != implementingClass)
				{
					throw new InvalidOperationException(string.Format("Name `{0}` is already registered", name));
				}

                return td;
            }
            else
            {
                var nextId = _knownTypes.Count;
                var typeDesc = new TypeDescriptor()
                {
                    ID = nextId,
                    Name = name
                };

                RegisterType(typeDesc, implementingClass);
                return typeDesc;
            }

        }

        private void RegisterType(TypeDescriptor td, Type implementingClass)
        {
            _knownTypesIndexes.Add(td.Name, td.ID);
            _knownTypes.Add(new KnownType()
                {
                    Descriptor = td,
                    SystemType = implementingClass
                });
        }

        public void RegisterAliasFor(TypeDescriptor td, string alias)
        {
            _knownTypesIndexes[alias] = td.ID;
        }

        public TypeDescriptor GetTypeByFrameworkType(Type type)
        {
            var kt = _knownTypes.First(x => x.SystemType == type);
            return kt.Descriptor;
        }

        public bool IsKnownType(Type type)
        {
            return _knownTypes.Any(x => x.SystemType == type);
        }

        public bool IsKnownType(string typeName)
        {
            var nameToUpper = typeName.ToUpperInvariant();
            return _knownTypes.Any(x => x.Descriptor.Name.ToUpperInvariant() == nameToUpper);
        }

        public Type NewInstanceHandler 
        { 
            get
            {
                return _dynamicFactory;
            }

            set
            {
                if (value.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                    .Where(x => x.GetCustomAttributes(false).Any(y => y is ScriptConstructorAttribute))
                    .Any())
                {
                    _dynamicFactory = value;
                }
                else
                {
                    throw new InvalidOperationException("Class " + value.ToString() + " can't be registered as New handler");
                }
            }
        }

        public TypeDescriptor GetTypeDescriptorFor(IValue typeTypeValue)
        {
            if (typeTypeValue.DataType != DataType.Type)
                throw RuntimeException.InvalidArgumentType();

            var ttVal = typeTypeValue.GetRawValue() as TypeTypeValue;

            System.Diagnostics.Debug.Assert(ttVal != null, "value must be of type TypeTypeValue");

            return ttVal.Value;

        }

        #endregion

    }

    public static class TypeManager
    {
        private static ITypeManager _instance;
        private static Dictionary<Type, TypeFactory> _factories;

        internal static void Initialize(ITypeManager instance)
        {
            _instance = instance;
            _factories = new Dictionary<Type, TypeFactory>();
        }

        public static ITypeManager Instance => _instance;

        public static Type GetImplementingClass(int typeId)
        {
            return _instance.GetImplementingClass(typeId);
        }

        public static TypeDescriptor GetTypeByName(string name)
        {
            return _instance.GetTypeByName(name);
        }

        public static TypeDescriptor RegisterType(string name, Type implementingClass)
        {
            return _instance.RegisterType(name, implementingClass);
        }

        public static void RegisterAliasFor(TypeDescriptor td, string alias)
        {
            _instance.RegisterAliasFor(td, alias);
        }

        public static TypeDescriptor GetTypeById(int id)
        {
            var type = _instance.GetTypeById(id);
            return type;
        }

        public static bool IsKnownType(Type type)
        {
            return _instance.IsKnownType(type);
        }

        public static bool IsKnownType(string typeName)
        {
            return _instance.IsKnownType(typeName);
        }

        public static TypeDescriptor GetTypeByFrameworkType(Type type)
        {
            return _instance.GetTypeByFrameworkType(type);
        }

        public static TypeDescriptor GetTypeDescriptorFor(IValue typeTypeValue)
        {
            return _instance.GetTypeDescriptorFor(typeTypeValue);
        }

        public static Type NewInstanceHandler
        {
            get
            {
                return _instance.NewInstanceHandler;
            }
            set
            {
                _instance.NewInstanceHandler = value;
            }
        }

        public static TypeFactory GetFactoryFor(string typeName)
        {
            int typeId;
            Type clrType;
            try
            {
                typeId = TypeManager.GetTypeByName(typeName).ID;
                clrType = TypeManager.GetImplementingClass(typeId);
            }
            catch (RuntimeException e)
            {
                if (NewInstanceHandler != null)
                {
                    clrType = NewInstanceHandler;
                }
                else
                {
                    throw new RuntimeException("Конструктор не найден (" + typeName + ")", e);
                }
            }

            if(!_factories.TryGetValue(clrType, out var factory))
            {
                factory = new TypeFactory(clrType);
                _factories[clrType] = factory;
            }

            return factory;
        }
    }

}
