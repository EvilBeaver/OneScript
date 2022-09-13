/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Reflection;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Runtime.Binding;
using OneScript.Values;

namespace OneScript.Compilation.Binding
{
    public class SymbolScope
    {
        public IndexedNameValueCollection<IMethodSymbol> Methods { get; } =
            new IndexedNameValueCollection<IMethodSymbol>();

        public IndexedNameValueCollection<IVariableSymbol> Variables { get; } =
            new IndexedNameValueCollection<IVariableSymbol>();
        
        public static SymbolScope FromObject(BslObjectValue target)
        {
            var scope = new SymbolScope();

            var type = target.GetType();
            foreach (var info in type.GetMethods())
            {
                var attr = info.GetCustomAttribute<ContextMethodAttribute>();
                if(attr == null)
                    continue;
                
                var symbol = new BslMethodSymbol
                {
                    Method = new ContextMethodInfo(info),
                };

                scope.Methods.Add(symbol, symbol.Name, symbol.Alias);
            }
            
            foreach (var info in type.GetProperties())
            {
                var attr = info.GetCustomAttribute<ContextMethodAttribute>();
                if(attr == null)
                    continue;
                
                var symbol = new BslPropertySymbol
                {
                    Property = new ContextPropertyInfo(info),
                };

                scope.Variables.Add(symbol, symbol.Name, symbol.Alias);
            }

            return scope;
        }

        public static SymbolScope FromContext(IRuntimeContextInstance target)
        {
            var scope = new SymbolScope();
            for (int i = 0; i < target.GetPropCount(); i++)
            {
                var targetProp = target.GetPropertyInfo(i);
                scope.Variables.Add(new BslPropertySymbol { Property = targetProp }, targetProp.Name, targetProp.Alias);
            }
            
            for (int i = 0; i < target.GetMethodsCount(); i++)
            {
                var targetMeth = target.GetMethodInfo(i);
                scope.Methods.Add(new BslMethodSymbol { Method = targetMeth }, targetMeth.Name, targetMeth.Alias);
            }

            return scope;
        }

        public static SymbolScope FromContext(IContext target)
        {
            var scope = new SymbolScope();
            
            foreach (var info in target.GetMethods())
            {
                var symbol = new BslMethodSymbol
                {
                    Method = info
                };

                scope.Methods.Add(symbol, symbol.Name, symbol.Alias);
            }
            
            foreach (var info in target.GetProperties())
            {
                var symbol = new BslPropertySymbol
                {
                    Property = info
                };

                scope.Variables.Add(symbol, symbol.Name, symbol.Alias);
            }

            return scope;
        }
    }
}