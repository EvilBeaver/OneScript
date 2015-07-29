/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Contexts
{
    public class UserScriptContextInstance : ScriptDrivenObject
    {
        LoadedModule _module;
        Dictionary<string, int> _ownPropertyIndexes;
        List<IValue> _ownProperties;
        
        internal UserScriptContextInstance(LoadedModule module) : base(module)
        {
            _module = module;
        }

        internal UserScriptContextInstance(LoadedModule module, string asObjectOfType)
            : base(module, true)
        {
            DefineType(TypeManager.GetTypeByName(asObjectOfType));
            _module = module;
        }

        public void AddProperty(string name, IValue value)
        {
            if(_ownProperties == null)
            {
                _ownProperties = new List<IValue>();
                _ownPropertyIndexes = new Dictionary<string, int>();
            }

            var newIndex = _ownProperties.Count;
            _ownPropertyIndexes.Add(name, newIndex);
            _ownProperties.Add(value);

        }

        protected override int GetMethodCount()
        {
            return 0;
        }

        protected override int GetVariableCount()
        {
            if (_ownProperties == null)
                return 0;
            else
                return _ownProperties.Count;
        }

        protected override void UpdateState()
        {
        }

        protected override bool IsOwnPropReadable(int index)
        {
            if (_ownProperties == null)
                return false;

            if (index >= 0 && index < _ownProperties.Count)
                return true;
            else
                return false;
        }

        protected override IValue GetOwnPropValue(int index)
        {
            return _ownProperties[index];
        }

        #region IReflectableContext Members

        public override IEnumerable<VariableInfo> GetProperties()
        {
            foreach (var item in _module.ExportedProperies)
            {
                var vi = new VariableInfo();
                vi.Identifier = item.SymbolicName;
                vi.Index = item.Index;
                vi.Type = SymbolType.ContextProperty;
                
                yield return vi;
            }
        }

        public override IEnumerable<MethodInfo> GetMethods()
        {
            foreach (var item in _module.ExportedMethods)
            {
                yield return GetMethodInfo(item.Index);
            }
        }

        #endregion
    }
}
