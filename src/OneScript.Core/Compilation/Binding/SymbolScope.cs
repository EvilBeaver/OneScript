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
                
                var symbol = new BslBoundMethodSymbol
                {
                    Method = new ContextMethodInfo(info),
                    Target = target
                };

                scope.AddMethod(symbol);
            }
            
            foreach (var info in type.GetProperties())
            {
                var attr = info.GetCustomAttribute<ContextMethodAttribute>();
                if(attr == null)
                    continue;
                
                var symbol = new BslBoundPropertySymbol
                {
                    Property = new ContextPropertyInfo(info),
                    Target = target
                };

                scope.AddVariable(symbol);
            }

            return scope;
        }

        public static SymbolScope FromContext(IRuntimeContextInstance target)
        {
            var scope = new SymbolScope();
            for (int i = 0; i < target.GetPropCount(); i++)
            {
                scope.AddVariable(new BslBoundPropertySymbol
                {
                    Target = target,
                    Property = target.GetPropertyInfo(i)
                });
            }
            
            for (int i = 0; i < target.GetMethodsCount(); i++)
            {
                scope.AddMethod(new BslBoundMethodSymbol
                {
                    Target = target,
                    Method = target.GetMethodInfo(i)
                });
            }

            return scope;
        }

        public static SymbolScope FromContext(IContext target)
        {
            var scope = new SymbolScope();
            
            foreach (var info in target.GetMethods())
            {
                var symbol = new BslBoundMethodSymbol
                {
                    Method = info,
                    Target = target
                };

                scope.AddMethod(symbol);
            }
            
            foreach (var info in target.GetProperties())
            {
                var symbol = new BslBoundPropertySymbol
                {
                    Property = info,
                    Target = target
                };

                scope.AddVariable(symbol);
            }

            return scope;
        }
    }
}