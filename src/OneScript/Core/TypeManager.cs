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
