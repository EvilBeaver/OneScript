/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Runtime.Binding;
using OneScript.Values;

namespace OneScript.Compilation.Binding
{
    public class SymbolScope
    {
        private IndexedNameValueCollection<IMethodSymbol> Methods { get; } =
            new IndexedNameValueCollection<IMethodSymbol>();

        private IndexedNameValueCollection<IVariableSymbol> Variables { get; } =
            new IndexedNameValueCollection<IVariableSymbol>();


        public int DefineVariable(IVariableSymbol symbol)
        {
            var index = AddItem(Variables, symbol);
            if (index == -1)
                throw new BindingException(LocalizedErrors.DuplicateVarDefinition(symbol.Name));

            if (!AddAlias(Variables, index, symbol))
                throw new BindingException(LocalizedErrors.DuplicateVarDefinition(symbol.Alias));

            return index;
        }

        public int DefineMethod(IMethodSymbol symbol)
        {
            var index = AddItem(Methods, symbol);
            if (index == -1)
                throw new BindingException(LocalizedErrors.DuplicateMethodDefinition(symbol.Name));

            if (!AddAlias(Methods, index, symbol))
                throw new BindingException(LocalizedErrors.DuplicateMethodDefinition(symbol.Alias));

            return index;
        }

        public IMethodSymbol GetMethod(int index) => Methods[index];
        
        public IMethodSymbol GetMethod(string name) => Methods[name];

        public IEnumerable<IMethodSymbol> GetMethods() => Methods;

        public int MethodCount => Methods.Count;

        public IVariableSymbol GetVariable(int index) => Variables[index];
        
        public IVariableSymbol GetVariable(string name) => Variables[name];
        
        public IEnumerable<IVariableSymbol> GetVariables() => Variables;

        public int VariableCount => Variables.Count;

        public int GetVariableIndex(string name)
        {
            return Variables.IndexOf(name);
        }

        public int GetMethodIndex(string name)
        {
            return Methods.IndexOf(name);
        }

        private static int AddItem<T>(IndexedNameValueCollection<T> storage, T item)
            where T : ISymbol
        {
            try
            {
                return storage.Add(item, item.Name);
            }
            catch (InvalidOperationException)
            {
                return -1;
            }
        }
        
        private static bool AddAlias<T>(IndexedNameValueCollection<T> storage, int index, T item)
            where T : ISymbol
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.Alias))
                    return true;
                
                storage.AddName(index, item.Alias);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
        
        #region Static part
        
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
        
        #endregion
    }
}