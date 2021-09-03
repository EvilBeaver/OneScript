/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Reflection;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Values;

namespace OneScript.Native.Compiler
{
    public class SymbolScope
    {
        private readonly IndexedNameValueCollection<MethodSymbol> _methods =
            new IndexedNameValueCollection<MethodSymbol>();

        private readonly IndexedNameValueCollection<VariableSymbol> _variables =
            new IndexedNameValueCollection<VariableSymbol>();


        public IndexedNameValueCollection<MethodSymbol> Methods => _methods;

        public IndexedNameValueCollection<VariableSymbol> Variables => _variables;

        public static SymbolScope FromContext(BslObjectValue target)
        {
            var scope = new SymbolScope();

            var type = target.GetType();
            foreach (var info in type.GetMethods())
            {
                var attr = info.GetCustomAttribute<ContextMethodAttribute>();
                if(attr == null)
                    continue;
                
                var symbol = new MethodSymbol
                {
                    Name = attr.GetName(),
                    Alias = attr.GetAlias(),
                    MemberInfo = info,
                    Target = target
                };

                scope.AddMethod(symbol);
            }
            
            foreach (var info in type.GetProperties())
            {
                var attr = info.GetCustomAttribute<ContextMethodAttribute>();
                if(attr == null)
                    continue;
                
                var symbol = new PropertySymbol
                {
                    Name = attr.GetName(),
                    Alias = attr.GetAlias(),
                    MemberInfo = info,
                    Target = target
                };

                scope.AddVariable(symbol);
            }

            return scope;
        }
        
    }
}