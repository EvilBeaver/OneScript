/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Runtime.Binding;

namespace OneScript.Compilation.Binding
{
    public static class HelperExtensions
    {
        public static SymbolScope AddVariable(this SymbolScope scope, IVariableSymbol symbol)
        {
            scope.Variables.Add(symbol, symbol.Name, symbol.Alias);
            return scope;
        }

        // удалить после переноса RuntimeEnvironment в Core
        public static IVariableSymbol MakePropSymbol(this BslPropertyInfo propertyInfo, object target)
        {
            return new BslBoundPropertySymbol
            {
                Property = propertyInfo,
                Target = target
            };
        }
        
        public static SymbolScope AddMethod(this SymbolScope scope, IMethodSymbol symbol)
        {
            scope.Methods.Add(symbol, symbol.Name, symbol.Alias);
            return scope;
        }
    }
}