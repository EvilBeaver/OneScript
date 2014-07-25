using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public class TypeManager
    {
        private Dictionary<string, DataType> _registeredTypes = new Dictionary<string,DataType>(StringComparer.InvariantCultureIgnoreCase);

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
            DataType t = DataType.CreateSimple(name, alias);
            RegisterType(t);
            return t;
        }

        public DataType RegisterObjectType(string name)
        {
            return RegisterObjectType(name, null);
        }

        public DataType RegisterObjectType(string name, string alias)
        {
            DataType t = DataType.CreateObject(name, alias);
            RegisterType(t);
            return t;
        }

        private void RegisterType(DataType dataType)
        {
            _registeredTypes.Add(dataType.Name, dataType);
            if(dataType.Alias != null)
            {
                _registeredTypes.Add(dataType.Alias, dataType);
            }
        }

        public DataType this[string name]
        {
            get
            {
                DataType typeInstance;
                if (_registeredTypes.TryGetValue(name, out typeInstance))
                {
                    return typeInstance;
                }

                return null;
            }
        }
            
    }
}
