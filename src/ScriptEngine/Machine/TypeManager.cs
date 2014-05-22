using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    [AttributeUsage(AttributeTargets.Class)]
    class ContextClassAttribute : Attribute
    {
        string _name;

        public ContextClassAttribute(string typeName)
        {
            _name = typeName;
        }

        public string GetName()
        {
            return _name;
        }
    }

    interface ITypeManager
    {
        Type GetImplementingClass(int typeId);
        TypeDescriptor GetTypeByName(string name);
        void RegisterType(string name, Type implementingClass);
    }

    class StandartTypeManager : ITypeManager
    {
        Dictionary<string, TypeDescriptor> _knownTypes = new Dictionary<string,TypeDescriptor>(StringComparer.InvariantCultureIgnoreCase);
        List<Type> _implementations = new List<Type>();

        public StandartTypeManager()
        {
            foreach (var item in Enum.GetNames(typeof(DataType)))
            {
                RegisterType(item, typeof(object));
            }
        }

        #region ITypeManager Members

        public Type GetImplementingClass(int typeId)
        {
            return _implementations[typeId];
        }

        public TypeDescriptor GetTypeByName(string name)
        {
            return _knownTypes[name];
        }

        public void RegisterType(string name, Type implementingClass)
        {
            if (_knownTypes.ContainsKey(name))
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

                _knownTypes.Add(name, typeDesc);
                _implementations.Add(implementingClass);
            }

        }

        #endregion
    }

    static class TypeManager
    {
        private static ITypeManager _instance;

        public static void Initialize(ITypeManager instance)
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
    }

}
