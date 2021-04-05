/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Dynamic;
using System.Linq.Expressions;
using OneScript.Commons;
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