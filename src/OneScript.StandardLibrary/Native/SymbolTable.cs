/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Reflection;

namespace OneScript.StandardLibrary.Native
{
    internal class SymbolTable
    {
        private List<SymbolScope> _scopes = new List<SymbolScope>();
        
        public SymbolScope GetScope(int index) => _scopes[index];

        public SymbolScope TopScope() => _scopes.Count == 0?null:_scopes[_scopes.Count - 1];
        
        public void AddScope(SymbolScope scope)
        {
            _scopes.Add(scope);
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
                        ContextIndex = i,
                        CodeIndex = idx
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
                        ContextIndex = i,
                        CodeIndex = idx
                    };
                    return true;
                }
            }

            binding = default;
            return false;
        }

    }
}
