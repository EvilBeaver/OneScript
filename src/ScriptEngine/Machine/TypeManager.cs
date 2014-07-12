using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        void RegisterType(string name, Type implementingClass);
        bool IsKnownType(Type type);
    }

    class StandartTypeManager : ITypeManager
    {
        private Dictionary<string, int> _knownTypesIndexes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private List<KnownType> _knownTypes = new List<KnownType>();

        private struct KnownType
        {
            public Type SystemType;
            public TypeDescriptor Descriptor;
        }

        public StandartTypeManager()
        {
            foreach (var item in Enum.GetValues(typeof(DataType)))
            {
                var td = TypeDescriptor.FromDataType((DataType)item);
                RegisterType(td, typeof(DataType));
            }
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

        public void RegisterType(string name, Type implementingClass)
        {
            if (_knownTypesIndexes.ContainsKey(name))
            {
                var td = GetTypeByName(name);
                if (GetImplementingClass(td.ID) != implementingClass)
                    throw new InvalidOperationException("Name already registered");

                return;
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
            }

        }

        private void RegisterType(TypeDescriptor td, Type implementingClass)
        {
            _knownTypesIndexes.Add(td.Name, _knownTypes.Count);
            _knownTypes.Add(new KnownType()
                {
                    Descriptor = td,
                    SystemType = implementingClass
                });
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

        public static void RegisterType(string name, Type implementingClass)
        {
            _instance.RegisterType(name, implementingClass);
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
    }

}
