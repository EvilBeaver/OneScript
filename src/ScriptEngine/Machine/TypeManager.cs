using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContextClassAttribute : Attribute
    {
        string _name;
        string _alias;

        public ContextClassAttribute(string typeName, string typeAlias = "")
        {
            _name = typeName;
            _alias = typeAlias;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetAlias()
        {
            return _alias;
        }

    }

    interface ITypeManager
    {
        Type GetImplementingClass(int typeId);
        TypeDescriptor GetTypeByName(string name);
        TypeDescriptor GetTypeByFrameworkType(Type type);
        TypeDescriptor RegisterType(string name, Type implementingClass);
        void RegisterAliasFor(TypeDescriptor td, string alias);
        bool IsKnownType(Type type);
        Type NewInstanceHandler { get; set; }
    }

    class StandartTypeManager : ITypeManager
    {
        private Dictionary<string, int> _knownTypesIndexes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private List<KnownType> _knownTypes = new List<KnownType>();
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
                string alias;
                switch (typeEnum)
                {
                    case DataType.Undefined:
                        alias = "Неопределено";
                        break;
                    case DataType.Boolean:
                        alias = "Булево";
                        break;
                    case DataType.String:
                        alias = "Строка";
                        break;
                    case DataType.Date:
                        alias = "Дата";
                        break;
                    case DataType.Number:
                        alias = "Число";
                        break;
                    case DataType.Type:
                        alias = "Тип";
                        break;
                    case DataType.Object:
                        alias = "$_";
                        break;
                    default:
                        continue;
                }

                var td = TypeDescriptor.FromDataType(typeEnum);
                RegisterType(td, typeof(DataType));
                RegisterAliasFor(td, alias);

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
            var ktIndex = _knownTypesIndexes[name];
            return _knownTypes[ktIndex].Descriptor;
        }

        public TypeDescriptor RegisterType(string name, Type implementingClass)
        {
            if (_knownTypesIndexes.ContainsKey(name))
            {
                var td = GetTypeByName(name);
                if (GetImplementingClass(td.ID) != implementingClass)
                    throw new InvalidOperationException("Name already registered");

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

        #endregion

    }

    public static class TypeManager
    {
        private static ITypeManager _instance;

        internal static void Initialize(ITypeManager instance)
        {
            _instance = instance;
        }

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

        public static int GetTypeIDByName(string name)
        {
            var type = _instance.GetTypeByName(name);
            return type.ID;
        }

        public static bool IsKnownType(Type type)
        {
            return _instance.IsKnownType(type);
        }

        public static TypeDescriptor GetTypeByFrameworkType(Type type)
        {
            return _instance.GetTypeByFrameworkType(type);
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

        public static Type GetFactoryFor(string typeName)
        {
            int typeId;
            Type clrType;
            try
            {
                typeId = TypeManager.GetTypeIDByName(typeName);
                clrType = TypeManager.GetImplementingClass(typeId);
            }
            catch (KeyNotFoundException)
            {
                if (NewInstanceHandler != null)
                {
                    clrType = NewInstanceHandler;
                }
                else
                {
                    throw new RuntimeException("Конструктор не найден (" + typeName + ")");
                }
            }

            return clrType;
        }
    }

}
