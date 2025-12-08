/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;

namespace ScriptEngine.Machine.Contexts
{
    public class DynamicPropertiesHolder
    {
        private readonly IndexedNameValueCollection<BslPropertyInfo> _propDefs =
            new IndexedNameValueCollection<BslPropertyInfo>();

        public delegate BslPropertyInfo PropertyInfoFactory(int index, string name, bool canRead, bool canWrite);
        
        private readonly PropertyInfoFactory _infoFactory = MakeProperty;

        public DynamicPropertiesHolder()
        {
        }

        public DynamicPropertiesHolder(PropertyInfoFactory infoFactory)
        {
            _infoFactory = infoFactory;
        }
        
        public int RegisterProperty(string name, bool canRead = true, bool canWrite = true)
        {
            var index = _propDefs.IndexOf(name); 
            if (index != -1)
            {
                return index;
            }

            if (!IsValidIdentifier(name))
            {
                throw RuntimeException.InvalidArgumentValue();
            }

            index = _propDefs.Count;
            return _propDefs.Add(_infoFactory(index, name, canRead, canWrite), name);
        }
        
        public int RegisterProperty(BslPropertyInfo prop)
        {
            var index = _propDefs.IndexOf(prop.Name);
            if (index != -1)
            {
                throw new ArgumentException($"Property {prop.Name} already exists");
            }
            
            return _propDefs.Add(prop, prop.Name);
        }

        private static BslPropertyInfo MakeProperty(int index, string name, bool canRead, bool canWrite)
        {
            return BslPropertyBuilder.Create()
                .Name(name)
                .SetDispatchingIndex(index)
                .CanRead(canRead)
                .CanWrite(canWrite)
                .Build();
        }

        public void RemoveProperty(string name)
        {
            _propDefs.RemoveValue(name);
        }

        public void ClearProperties()
        {
            _propDefs.Clear();
        }

        public int GetPropertyNumber(string name)
        {
            var index = _propDefs.IndexOf(name);
            if (index != -1)
                return index;
            
            throw PropertyAccessException.PropNotFoundException(name);
        }

        public string GetPropertyName(int idx)
        {
            return _propDefs[idx].Name;
        }

        public IEnumerable<KeyValuePair<string, int>> GetProperties()
        {
            return _propDefs.GetIndex();
        }

        public int Count => _propDefs.Count;

        public BslPropertyInfo this[int index] => _propDefs[index];
        
        public BslPropertyInfo this[string name] => _propDefs[name];

        private bool IsValidIdentifier(string name)
        {
            return Utils.IsValidIdentifier(name);
        }

    }
}
