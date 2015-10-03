using OneScript.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OneScript.Core
{
    public class TypeManager
    {
        private Dictionary<string, int> _typeNames = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<TypeId, int> _typeIds = new Dictionary<TypeId, int>();
        private List<DataType> _types = new List<DataType>();
        

        public TypeManager()
        {
            RegisterType(BasicTypes.Number);
            RegisterType(BasicTypes.String);
            RegisterType(BasicTypes.Date);
            RegisterType(BasicTypes.Boolean);
            RegisterType(BasicTypes.Undefined);
            RegisterType(BasicTypes.Type);
            RegisterType(BasicTypes.Null);
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
            var t = DataType.CreateType(name, alias);
            t.Constructor = constructor;
            RegisterType(t);
            return t;
        }

        public DataType RegisterSimpleType(string name, string alias, TypeId id, DataTypeConstructor constructor)
        {
            var t = DataType.CreateType(name, alias, id);
            t.Constructor = constructor;
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
            DataType t = DataType.CreateType(name, alias);
            t.Constructor = constructor;
            t.IsObject = true;
            RegisterType(t);
            return t;
        }

        public DataType RegisterObjectType(string name, string alias, TypeId id, DataTypeConstructor constructor)
        {
            DataType t = DataType.CreateType(name, alias, id);
            t.Constructor = constructor;
            t.IsObject = true;
            RegisterType(t);
            return t;
        }

        private void RegisterType(DataType dataType)
        {
            int newIndex = _types.Count;
            _typeIds.Add(dataType.ID, newIndex);
            try
            {
                _typeNames.Add(dataType.Name, newIndex);
            }
            catch (ArgumentException)
            {
                _typeIds.Remove(dataType.ID);
                throw;
            }

            if(dataType.Alias != null && !string.Equals(dataType.Alias, dataType.Name, StringComparison.Ordinal))
            {
                try
                {
                    _typeNames.Add(dataType.Alias, newIndex);
                }
                catch (ArgumentException)
                {
                    _typeNames.Remove(dataType.Name);
                    throw;
                }
            }

            _types.Add(dataType);
        }

        public DataType GetByName(string name)
        {
            int index;
            if (_typeNames.TryGetValue(name, out index))
            {
                return _types[index];
            }

            return null;
        }

        public DataType GetById(TypeId id)
        {
            int index;
            if (_typeIds.TryGetValue(id, out index))
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
        
    }
}
