/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Contexts.Internal;
using OneScript.Localization;
using OneScript.Values;

namespace OneScript.Compilation
{
    internal class GlobalPropertiesHolder : IContext
    {
        private IndexedNameValueCollection<BslValue> _values= new IndexedNameValueCollection<BslValue>();
        private List<ExternalPropertyInfo> _properties = new List<ExternalPropertyInfo>();

        public BslPropertyInfo Register(BilingualString names, BslValue value)
        {
            var index = _values.Add(value, names.Russian, names.English);
            var prop = new ExternalPropertyInfo(names.Russian, names.English, this, index);
            Debug.Assert(index == _properties.Count);
            _properties.Add(prop);
            return prop;
        }
        
        public BslPropertyInfo Register(string name, BslValue value)
        {
            var index = _values.Add(value, name);
            var prop = new ExternalPropertyInfo(name, null, this, index);
            _properties.Add(prop);
            return prop;
        }

        public bool HasProperty(BilingualString name)
        {
            return _values.IndexOf(name.Russian) != -1 || _values.IndexOf(name.English) != -1;
        }

        public BslValue GetPropValue(int index) => _values[index];

        public void SetPropValue(int index, BslValue value) =>
            _values[index] = value ?? throw new ArgumentNullException(nameof(value));

        #region Context Implementation

        public IReadOnlyList<BslPropertyInfo> GetProperties() => _properties;

        public IReadOnlyList<BslMethodInfo> GetMethods() => Array.Empty<BslMethodInfo>();

        public BslPropertyInfo FindProperty(string name)
        {
            var idx = _values.IndexOf(name);
            return idx == -1 ? default : _properties[idx];
        }

        public BslMethodInfo FindMethod(string name) => default;
        
        public bool DynamicMethodSignatures => false;
        
        public void SetPropValue(BslPropertyInfo property, BslValue value)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            
            var externalProp = property as ExternalPropertyInfo;
            Debug.Assert(externalProp != null, property.GetType().ToString());
            
            SetPropValue(externalProp.DispatchId, value);
        }

        public BslValue GetPropValue(BslPropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            
            var externalProp = property as ExternalPropertyInfo;
            return GetPropValue(externalProp.DispatchId);
        }

        public void CallAsProcedure(BslMethodInfo method, IReadOnlyList<BslValue> arguments)
        {
            throw new ArgumentException();
        }

        public BslValue CallAsFunction(BslMethodInfo method, IReadOnlyList<BslValue> arguments)
        {
            throw new ArgumentException();
        }

        #endregion
    }
}