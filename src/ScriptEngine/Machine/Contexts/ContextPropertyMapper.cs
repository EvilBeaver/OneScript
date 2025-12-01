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
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Language;

namespace ScriptEngine.Machine.Contexts
{
    public class ContextPropertyMapper<TInstance>
    {
        private List<PropertyTarget<TInstance>> _properties;
        private IdentifiersTrie<int> _propertyNumbers;
        
        private readonly object _locker = new object();

        private void Init()
        {
            if (_properties != null) 
                return;

            lock (_locker)
            {
                if (_properties == null)
                {
                    var localProps = MapProperties();
                    _propertyNumbers = new IdentifiersTrie<int>();
                    for (int idx = 0; idx < localProps.Count; ++idx)
                    {
                        var propInfo = localProps[idx];

                        _propertyNumbers.Add(propInfo.Name, idx);
                        if (propInfo.Alias != null)
                            _propertyNumbers.Add(propInfo.Alias, idx);
                    }

                    _properties = localProps;
                }
            }
        }

        private static List<PropertyTarget<TInstance>> MapProperties()
        {
            var mappedProperties = new List<PropertyTarget<TInstance>>();
            foreach (var propertyInfo in typeof(TInstance).GetProperties()
                         .Where(x => Attribute.IsDefined(x, typeof(ContextPropertyAttribute))))
            {
                var propertyMarkup = propertyInfo.GetCustomAttribute<ContextPropertyAttribute>();
                var bslProp = new ContextPropertyInfo(propertyInfo, propertyMarkup);
                var mainMapping = new PropertyTarget<TInstance>(bslProp);
                mappedProperties.Add(mainMapping);

                foreach (var deprecation in propertyInfo.GetCustomAttributes<DeprecatedNameAttribute>())
                {
                    var deprecatedMarkup = new ContextPropertyAttribute(deprecation.Name)
                    {
                        CanRead = bslProp.CanRead,
                        CanWrite = bslProp.CanWrite,
                        IsDeprecated = true,
                        ThrowOnUse = deprecation.ThrowOnUse
                    };

                    bslProp = new ContextPropertyInfo(propertyInfo, deprecatedMarkup);
                    var deprecatedMapping = new PropertyTarget<TInstance>(bslProp);
                    mappedProperties.Add(deprecatedMapping);
                }
            }

            return mappedProperties;
        }

        public bool ContainsProperty(string name)
        {
            return GetPropertyIndex(name) >= 0;
        }

        public int FindProperty(string name)
        {
            var idx = GetPropertyIndex(name);
            if (idx < 0)
                throw PropertyAccessException.PropNotFoundException(name);

            return idx;
        }

        public PropertyTarget<TInstance> GetProperty(int index)
        {
            Init();
            return _properties[index];
        }

        public IEnumerable<BslPropertyInfo> GetProperties()
        {
            Init();
            return _properties.Select(p => p.PropertyInfo);
        }

        public int Count
        {
            get
            {
                Init();
                return _properties.Count;
            }
        }

        private int GetPropertyIndex(string name)
        {
            Init();
            if (_propertyNumbers.TryGetValue(name, out var index))
                return index;

            return -1;
        }
    }
}
