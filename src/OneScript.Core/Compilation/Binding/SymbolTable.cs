/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Values;

namespace OneScript.Compilation.Binding
{
    public class SymbolTable
    {
        private struct BindingRecord
        {
            public SymbolScope scope;
            public object target;
        }
        
        private List<BindingRecord> _bindings = new List<BindingRecord>();
        
        public SymbolScope GetScope(int index) => _bindings[index].scope;

        public object GetBinding(int scopeIndex) => _bindings[scopeIndex].target;
        
        public int ScopeCount => _bindings.Count;
        
        public int PushScope(SymbolScope scope, object target)
        {
            var idx = _bindings.Count;
            _bindings.Add(new BindingRecord
            {
                scope = scope,
                target = target
            });
            
            return idx;
        }

        public void PopScope()
        {
            _bindings.RemoveAt(_bindings.Count - 1);
        }

        public bool FindVariable(string name, out SymbolBinding binding)
        {
            for (int i = _bindings.Count - 1; i >= 0; i--)
            {
                var scope = _bindings[i].scope;
                var idx = scope.Variables.IndexOf(name);
                if (idx >= 0)
                {
                    binding = new SymbolBinding
                    {
                        ScopeNumber = i,
                        MemberNumber = idx
                    };
                    return true;
                }
            }

            binding = default;
            return false;
        }
        
        public bool FindMethod(string name, out SymbolBinding binding)
        {
            for (int i = _bindings.Count - 1; i >= 0; i--)
            {
                var scope = _bindings[i].scope;
                var idx = scope.Methods.IndexOf(name);
                if (idx >= 0)
                {
                    binding = new SymbolBinding
                    {
                        ScopeNumber = i,
                        MemberNumber = idx
                    };
                    return true;
                }
            }

            binding = default;
            return false;
        }

        public void DefineMethod(IMethodSymbol symbol)
        {
            _bindings[ScopeCount - 1].scope.Methods.Add(symbol, symbol.Name, symbol.Alias);
        }
    }
}
