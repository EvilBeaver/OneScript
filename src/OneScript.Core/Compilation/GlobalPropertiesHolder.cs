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

        public void SetPropValue(int index, BslValue value) => _values[index] = value; 
        
        #region Context Implementation

        public IReadOnlyList<BslPropertyInfo> GetProperties()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<BslMethodInfo> GetMethods()
        {
            throw new NotImplementedException();
        }

        public BslPropertyInfo FindProperty(string name)
        {
            throw new NotImplementedException();
        }

        public BslMethodInfo FindMethod(string name)
        {
            throw new NotImplementedException();
        }

        public bool DynamicMethodSignatures => false;
        
        public void SetPropValue(BslPropertyInfo property, BslValue value)
        {
            throw new NotImplementedException();
        }

        public BslValue GetPropValue(BslPropertyInfo property)
        {
            throw new NotImplementedException();
        }

        public void CallAsProcedure(BslMethodInfo method, IReadOnlyList<BslValue> arguments)
        {
            throw new NotImplementedException();
        }

        public BslValue CallAsFunction(BslMethodInfo method, IReadOnlyList<BslValue> arguments)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}