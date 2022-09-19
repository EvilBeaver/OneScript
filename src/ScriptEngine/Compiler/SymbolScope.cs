/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Commons;
using OneScript.Compilation;
using OneScript.Contexts;
using OneScript.Localization;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    public class _SymbolScope
    {
        private readonly IndexedNameValueCollection<VariableInfo> _variables =
            new IndexedNameValueCollection<VariableInfo>();

        private readonly IndexedNameValueCollection<BslMethodInfo> _methods =
            new IndexedNameValueCollection<BslMethodInfo>();

        public BslMethodInfo GetMethod(int number)
        {
            return _methods[number];
        }

        public int GetVariableNumber(string name)
        {
            var index = _variables.IndexOf(name);
            if (index >= 0)
                return index;
            
            throw new SymbolNotFoundException(name);
        }

        public VariableInfo GetVariable(int number)
        {
            return _variables[number];
        }

        public int GetMethodNumber(string name)
        {
            var number = _methods.IndexOf(name);
            if(number >= 0)
            {
                return number;
            }
            throw new SymbolNotFoundException(name);
        }

        public bool IsVarDefined(string name)
        {
            return _variables.IndexOf(name) >= 0;
        }

        public bool IsMethodDefined(string name)
        {
            return _methods.IndexOf(name) >= 0;
        }

        public int DefineVariable(string name, string alias = null)
        {
            return DefineVariable(name, alias, SymbolType.Variable);
        }
        
        public int DefineProperty(string name, string alias = null)
        {
            return DefineVariable(name, alias, SymbolType.ContextProperty);
        }

        private int DefineVariable(string name, string alias, SymbolType symbolType)
        {
            if (IsVarDefined(name))
            {
                throw new InvalidOperationException($"Symbol already defined in the scope ({name})");
            }
            if (!string.IsNullOrEmpty(alias) && IsVarDefined(alias))
            {
                throw new InvalidOperationException($"Symbol already defined in the scope ({alias})");
            }

            var newIdx = _variables.Count;
            var item = new VariableInfo
            {
                Index = newIdx,
                Identifier = name,
                Alias = alias,
                Type = symbolType
            };

            _variables.Add(item, name, alias);

            return newIdx;
        }
        
        public int DefineMethod(BslMethodInfo method)
        {
            if (!IsMethodDefined(method.Name))
            {
                int newIdx = _methods.Add(method, method.Name, method.Alias);
                return newIdx;
            }
            else
            {
                throw new InvalidOperationException("Symbol already defined in the scope");
            }
        }

        public string GetVariableName(int number)
        {
            return _variables[number].Identifier;
        }

        public int VariableCount => _variables.Count;

        public int MethodCount => _methods.Count;

        [Obsolete("По факту нигде не используется")]
        public bool IsDynamicScope { get; set; }
    }

    class SymbolNotFoundException : CompilerException
    {
        public SymbolNotFoundException(string symbol) 
            : base(BilingualString.Localize($"Неизвестный символ: {symbol}", $"Unknown symbol {symbol}"))
        {
            Symbol = symbol;
        }

        public string Symbol { get; }
    }

}
