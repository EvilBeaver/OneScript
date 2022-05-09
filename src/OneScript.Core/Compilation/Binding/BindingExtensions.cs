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
        
        public static SymbolScope AddMethod(this SymbolScope scope, IMethodSymbol symbol)
        {
            scope.Methods.Add(symbol, symbol.Name, symbol.Alias);
            return scope;
        }

        public static SymbolBinding BindMethod(this SymbolTable table, object target, BslMethodInfo method)
        {
            if (table.TopScope() == null)
            {
                throw new InvalidOperationException("Scopes are not defined");
            }
            
            if (table.FindMethod(method.Name, out _))
            {
                throw new InvalidOperationException("Symbol already defined");
            }

            var symbol = new BslBoundMethodSymbol
            {
                Method = method,
                Name = method.Name,
                Alias = method.Alias,
                Target = target
            };
            
            var topIndex = table.ScopeCount - 1;
            var scope = table.TopScope();
            var index = scope.Methods.Add(symbol, method.Name, method.Alias);

            return new SymbolBinding
            {
                ScopeNumber = topIndex,
                MemberNumber = index
            };
        }

        public static SymbolBinding BindProperty(this SymbolTable table, object target, BslPropertyInfo property)
        {
            if (table.TopScope() == null)
            {
                throw new InvalidOperationException("Scopes are not defined");
            }
            
            if (table.FindVariable(property.Name, out _))
            {
                throw new InvalidOperationException("Symbol already defined");
            }

            var symbol = new BslBoundPropertySymbol
            {
                Name = property.Name,
                Alias = property.Alias,
                Property = property,
                Target = target
            };
            
            var topIndex = table.ScopeCount - 1;
            var scope = table.TopScope();
            var index = scope.Variables.Add(symbol, property.Name, property.Alias);

            return new SymbolBinding
            {
                ScopeNumber = topIndex,
                MemberNumber = index
            };
        }
        
        public static SymbolBinding BindVariable(this SymbolTable table, string name, string alias, Type type)
        {
            if (table.TopScope() == null)
            {
                throw new InvalidOperationException("Scopes are not defined");
            }
            
            if (table.FindVariable(name, out _))
            {
                throw new InvalidOperationException("Symbol already defined");
            }

            var symbol = new BoundVariableSymbol()
            {
                Name = name,
                Alias = alias,
                Type = type
            };
            
            var topIndex = table.ScopeCount - 1;
            var scope = table.TopScope();
            var index = scope.Variables.Add(symbol, name, alias);

            return new SymbolBinding
            {
                ScopeNumber = topIndex,
                MemberNumber = index
            };
        }
    }
}