using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OneScript.Core
{
    public class TypeManager
    {
        private Dictionary<string, int> _registeredTypes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private List<DataType> _types = new List<DataType>();
        private Dictionary<int, List<MethodInfo>> _constructors = new Dictionary<int, List<MethodInfo>>();

        public TypeManager()
        {
            RegisterType(BasicTypes.Number);
            RegisterType(BasicTypes.String);
            RegisterType(BasicTypes.Date);
            RegisterType(BasicTypes.Boolean);
            RegisterType(BasicTypes.Undefined);
            RegisterType(BasicTypes.Type);
        }

        public DataType RegisterSimpleType(string name)
        {
            return RegisterSimpleType(name, null);
        }

        public DataType RegisterSimpleType(string name, string alias)
        {
            return RegisterSimpleType(name, alias, null);
        }
        public DataType RegisterSimpleType(string name, string alias, DataTypeConstructor constructor)
        {
            var t = DataType.CreateSimpleType(name, alias, constructor);
            RegisterType(t);
            return t;
        }

        public DataType RegisterObjectType(string name)
        {
            return RegisterObjectType(name, null);
        }

        public DataType RegisterObjectType(string name, string alias)
        {
            return RegisterObjectType(name, alias, null);
        }

        public DataType RegisterObjectType(string name, string alias, DataTypeConstructor constructor)
        {
            DataType t = DataType.CreateObjectType(name, alias, constructor);
            RegisterType(t);
            return t;
        }

        private void RegisterType(DataType dataType)
        {
            int newIndex = _types.Count;
            _registeredTypes.Add(dataType.Name, newIndex);
            if(dataType.Alias != null)
            {
                try
                {
                    _registeredTypes.Add(dataType.Alias, newIndex);
                }
                catch (ArgumentException)
                {
                    _registeredTypes.Remove(dataType.Name);
                    throw;
                }
            }

            _types.Add(dataType);
        }

        public DataType GetByName(string name)
        {
            int index;
            if (_registeredTypes.TryGetValue(name, out index))
            {
                return _types[index];
            }

            return null;
        }

        public DataType this[string name]
        {
            get
            {
                return GetByName(name);
            }
        }
            
        internal void AddConstructorFor(string name, MethodInfo method)
        {
            int id = _registeredTypes[name];
            List<MethodInfo> constrList;
            if(!_constructors.TryGetValue(id, out constrList))
            {
                constrList = new List<MethodInfo>();
                _constructors[id] = constrList;
            }

            constrList.Add(method);
        }
    }
}
