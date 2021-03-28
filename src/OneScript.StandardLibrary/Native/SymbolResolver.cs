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
    internal class SymbolResolver
    {
        private readonly List<StrictTypedSymbolScope> _scopes = new List<StrictTypedSymbolScope>();
        private bool _variablesAdded = false;
        
        public void AddVariable(string name, Type type)
        {
            if (!_variablesAdded)
            {
                var scope = new StrictTypedSymbolScope();
                _scopes.Add(scope);
                _variablesAdded = true;
            }
            
            TopScope.AddVariable(name, type);
        }
        
        public void AddContext(IRuntimeContextInstance target)
        {
            if (_variablesAdded)
                throw new InvalidOperationException("Local variables scope is added");
            
            var scope = new StrictTypedSymbolScope();
            
            var reflector = new ClassBuilder(target.GetType());
            reflector.ExportDefaults();

            var type = reflector.Build();
            
            foreach (var info in type.GetMethods().Cast<ContextMethodInfo>())
            {
                scope.AddMethod(target, info);
            }
            
            foreach (var info in type.GetProperties().Cast<ContextPropertyInfo>())
            {
                scope.AddVariable(target, info);
            }
            
            _scopes.Add(scope);
        }

        public MethodSymbol GetMethod(string name)
        {
            for (int i = _scopes.Count - 1; i >= 0; i--)
            {
                var scope = _scopes[i];
                if (scope.TryGetMethod(name, out var result))
                    return result;
            }

            throw new CompilerException($"Method {name} is missing");
        }

        public VariableSymbol GetVariable(string name)
        {
            for (int i = _scopes.Count - 1; i >= 0; i--)
            {
                var scope = _scopes[i];
                if (scope.TryGetVariable(name, out var result))
                    return result;
            }

            throw new CompilerException($"Variable {name} is missing");
        }
        
        public bool TryGetVariable(string name, out VariableSymbol result)
        {
            for (int i = _scopes.Count - 1; i >= 0; i--)
            {
                var scope = _scopes[i];
                if (scope.TryGetVariable(name, out result))
                    return true;
            }

            result = default;
            return false;
        }

        private StrictTypedSymbolScope TopScope => _scopes[_scopes.Count - 1];
    }

    internal class StrictTypedSymbolScope
    {
        private readonly IndexedNameValueCollection<MethodSymbol> _methods =
            new IndexedNameValueCollection<MethodSymbol>();

        private readonly IndexedNameValueCollection<VariableSymbol> _variables =
            new IndexedNameValueCollection<VariableSymbol>();

        public void AddMethod(IRuntimeContextInstance context, ContextMethodInfo info)
        {
            _methods.Add(new MethodSymbol
            {
                Target = context,
                MethodInfo = info
            }, info.Name, info.Alias);
        }

        public bool TryGetMethod(string name, out MethodSymbol value)
        {
            return _methods.TryGetValue(name, out value);
        }

        public MethodSymbol GetMethod(int index)
        {
            return _methods[index];
        }

        public bool TryGetVariable(string name, out VariableSymbol value)
        {
            return _variables.TryGetValue(name, out value);
        }

        public VariableSymbol GetVariable(int index)
        {
            return _variables[index];
        }
        
        public void AddVariable(IRuntimeContextInstance context, ContextPropertyInfo info)
        {
            _variables.Add(new VariableSymbol
            {
                DataType = info.PropertyType,
                Owner = context,
                PropertyInfo = info
            }, info.Name, info.Alias);
        }
        
        public void AddVariable(string name, Type type)
        {
            _variables.Add(new VariableSymbol
            {
                DataType = type
            }, name);
        }
    }
}
