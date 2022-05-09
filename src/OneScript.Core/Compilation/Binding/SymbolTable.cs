/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace OneScript.Compilation.Binding
{
    public class SymbolTable
    {
        private List<SymbolScope> _scopes = new List<SymbolScope>();
        
        public SymbolScope GetScope(int index) => _scopes[index];

        public SymbolScope TopScope() => _scopes.Count == 0?null:_scopes[_scopes.Count - 1];

        public int ScopeCount => _scopes.Count;
        
        public void PushScope(SymbolScope scope)
        {
            _scopes.Add(scope);
        }

        public void PopScope()
        {
            _scopes.RemoveAt(_scopes.Count - 1);
        }

        public int IndexOf(SymbolScope scope)
        {
            return _scopes.IndexOf(scope);
        }
        
        public bool FindVariable(string name, out SymbolBinding binding)
        {
            for (int i = _scopes.Count - 1; i >= 0; i--)
            {
                var scope = _scopes[i];
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
            for (int i = _scopes.Count - 1; i >= 0; i--)
            {
                var scope = _scopes[i];
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
    }
}
