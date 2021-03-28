/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Reflection;

namespace OneScript.StandardLibrary.Native
{
    internal class SymbolScope
    {
        private readonly IndexedNameValueCollection<MethodSymbol> _methods =
            new IndexedNameValueCollection<MethodSymbol>();

        private readonly IndexedNameValueCollection<VariableSymbol> _variables =
            new IndexedNameValueCollection<VariableSymbol>();


        public IndexedNameValueCollection<MethodSymbol> Methods => _methods;

        public IndexedNameValueCollection<VariableSymbol> Variables => _variables;

        public static SymbolScope FromContext(IRuntimeContextInstance target)
        {
            var scope = new SymbolScope();
        
            var reflector = new ClassBuilder(target.GetType());
            reflector.ExportDefaults();

            var type = reflector.Build();
        
            foreach (var info in type.GetMethods().Cast<ContextMethodInfo>())
            {
                var symbol = new MethodSymbol
                {
                    Name = info.Name,
                    Alias = info.Alias,
                    MemberInfo = info,
                    Target = target
                };
                
                scope.Methods.Add(symbol, info.Name, info.Alias);
            }
        
            foreach (var info in type.GetProperties().Cast<ContextPropertyInfo>())
            {
                var symbol = new PropertySymbol
                {
                    Name = info.Name,
                    Alias = info.Alias,
                    MemberInfo = info
                };
                
                scope.Variables.Add(symbol, info.Name, info.Alias);
            }

            return scope;
        }

        public static void AddVariable(SymbolScope scope, string name, Type type)
        {
            var symbol = new VariableSymbol
            {
                Name = name,
                VariableType = type
            };
            
            scope.Variables.Add(symbol, name);
        }
    }
}