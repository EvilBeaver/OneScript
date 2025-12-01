/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using OneScript.Contexts;

namespace OneScript.Compilation.Binding
{
    public class SymbolTable : IEnumerable<SymbolScope>
    {
        private class BindingRecord
        {
            public SymbolScope Scope;
            public ScopeBindingDescriptor Descriptor;
        }
        
        private readonly List<BindingRecord> _bindings = new List<BindingRecord>();
        
        public SymbolScope GetScope(int index) => _bindings[index].Scope;

        public ScopeBindingDescriptor GetBinding(int scopeIndex) => _bindings[scopeIndex].Descriptor;

        public int ScopeCount => _bindings.Count;
        
        public int PushScope(SymbolScope scope, ScopeBindingDescriptor descriptor)
        {
            var idx = _bindings.Count;
            _bindings.Add(new BindingRecord
            {
                Scope = scope,
                Descriptor = descriptor
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
                var scope = _bindings[i].Scope;
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
        
        public bool TryFindMethodBinding(string name, out SymbolBinding binding)
        {
            for (int i = _bindings.Count - 1; i >= 0; i--)
            {
                var scope = _bindings[i].Scope;
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

        public bool TryFindMethod(string name, out IMethodSymbol method)
        {
            if (TryFindMethodBinding(name, out var binding))
            {
                method = GetMethod(binding);
                return true;
            }

            method = default;
            return false;
        }

        public SymbolBinding DefineMethod(IMethodSymbol symbol)
        {
            var index = _bindings[ScopeCount - 1].Scope.DefineMethod(symbol);
            return new SymbolBinding
            {
                ScopeNumber = ScopeCount - 1,
                MemberNumber = index
            };
        }
        
        public SymbolBinding DefineVariable(IVariableSymbol symbol)
        {
            var index = _bindings[ScopeCount - 1].Scope.DefineVariable(symbol);
            return new SymbolBinding
            {
                ScopeNumber = ScopeCount - 1,
                MemberNumber = index
            };
        }

        public IVariableSymbol GetVariable(SymbolBinding binding)
        {
            return GetScope(binding.ScopeNumber).Variables[binding.MemberNumber];
        }
        
        public IMethodSymbol GetMethod(SymbolBinding binding)
        {
            return GetScope(binding.ScopeNumber).Methods[binding.MemberNumber];
        }

        public IEnumerator<SymbolScope> GetEnumerator()
        {
            for (int i = 0; i < ScopeCount; i++)
                yield return _bindings[i].Scope;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
