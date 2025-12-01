/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Types;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class EnumerationContext : PropertyNameIndexAccessor, ICollectionContext<IValue>
    {
        private readonly IndexedNameValueCollection<EnumerationValue> _values;
        private readonly List<BslPropertyInfo> _definitions;
        private readonly TypeDescriptor _valuesType;
        private readonly HashSet<int> _checkedDeprecatedProps = new HashSet<int>();

        protected EnumerationContext(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) : base(typeRepresentation)
        {
            _valuesType = valuesType;
            _values = new IndexedNameValueCollection<EnumerationValue>();
            _definitions = new List<BslPropertyInfo>();
        }

        public void AddValue(EnumerationValue val)
        {
            var index = _values.Add(val, val.Name, val.Alias);
            
            var propertyBuilder = BslPropertyBuilder.Create()
                .SetNames(val.Name, val.Alias)
                .CanRead(true)
                .CanWrite(false)
                .SetDispatchingIndex(index);
                
            if (_valuesType != null)
                propertyBuilder.ReturnType(_valuesType.ImplementingClass);
            
            _definitions.Add(propertyBuilder.Build());
        }

        protected void AddValue(EnumerationValue val, BslPropertyInfo definition)
        {
            _values.Add(val, definition.Name, definition.Alias);
            _definitions.Add(definition);
        }

        public TypeDescriptor ValuesType => _valuesType;

        public EnumerationValue this[string name] => GetPropValueInternal(GetPropertyNumber(name));

        public override int GetPropCount()
        {
            return _values.Count;
        }

        public override int GetPropertyNumber(string name)
        {
            var id = _values.IndexOf(name); 
            
            return id == -1 ? base.GetPropertyNumber(name) : id;
        }

        public override bool IsPropReadable(int propNum)
        {
            return true;
        }

        public override IValue GetPropValue(int propNum)
        {
            return GetPropValueInternal(propNum);
        }
        
        private EnumerationValue GetPropValueInternal(int propNum)
        {
            WarnDeprecation(propNum);
            return _values[propNum];
        }
        
        private void WarnDeprecation(int propNum)
        {
            if (_checkedDeprecatedProps.Contains(propNum)) 
                return;
            
            if (GetPropertyInfo(propNum) is SystemPropertyInfo { IsDeprecated: true })
            {
                SystemLogger.Write($"Обращение к устаревшему свойству {GetPropertyInfo(propNum).Name}.");
            }
            
            _checkedDeprecatedProps.Add(propNum);
        }

        public override string GetPropName(int propNum)
        {
            return _values.NameOf(propNum);
        }
        
        public override BslPropertyInfo GetPropertyInfo(int propNum)
        {
            return _definitions[propNum];
        }

        protected IEnumerable<EnumerationValue> ValuesInternal => _values;

        #region ICollectionContext Members

        public int Count(IBslProcess process)
        {
            return _values.Count;
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _values)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        #endregion
    }
}
