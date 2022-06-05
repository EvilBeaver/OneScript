/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OneScript.Compilation;
using OneScript.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    public class CompilerContext : ICompilerContext
    {
        readonly List<SymbolScope> _scopeStack = new List<SymbolScope>();

        public void PushScope(SymbolScope scope)
        {
            _scopeStack.Add(scope);
        }

        public SymbolScope PopScope()
        {
            var idx = _scopeStack.Count - 1;
            if (idx >= 0)
            {
                var retVal = _scopeStack[idx];
                _scopeStack.RemoveAt(idx);
                return retVal;
            }
            else
            {
                throw new InvalidOperationException("No scopes defined");
            }
        }

        public SymbolScope Peek()
        {
            var idx = _scopeStack.Count - 1;
            if (idx >= 0)
            {
                return _scopeStack[idx];
            }
            else
            {
                throw new InvalidOperationException("No scopes defined");
            }
        }

        private SymbolBinding GetSymbol(string symbol, Func<string, SymbolScope, int> extract)
        {
            if (TryGetSymbol(symbol, extract, out var result))
            {
                return result;
            }

            throw new SymbolNotFoundException(symbol);
        }

        private bool TryGetSymbol(string symbol, Func<string, SymbolScope, int> extract, out SymbolBinding result)
        {
            for (int i = _scopeStack.Count - 1; i >= 0; i--)
            {
                var number = extract(symbol, _scopeStack[i]);
                if (number < 0)
                    continue;

                result = new SymbolBinding();
                result.CodeIndex = number;
                result.ContextIndex = i;
                return true;

            }

            result = default;
            return false;
        }

        private bool HasSymbol(Func<SymbolScope, bool> definitionCheck)
        {
            for (int i = _scopeStack.Count - 1; i >= 0; i--)
            {
                var isDefined = definitionCheck(_scopeStack[i]);
                if (isDefined)
                    return true;
            }

            return false;
        }

        public VariableBinding GetVariable(string name)
        {
            var sb = GetSymbol(name, ExtractVariableIndex);
            return new VariableBinding
            {
                type = _scopeStack[sb.ContextIndex].GetVariable(sb.CodeIndex).Type,
                binding = sb
            };
        }

        public SymbolBinding GetMethod(string name)
        {
            return GetSymbol(name, ExtractMethodIndex);
        }

        public bool TryGetMethod(string name, out SymbolBinding result)
        {
            return TryGetSymbol(name, ExtractMethodIndex, out result);
        }

        public bool TryGetVariable(string name, out VariableBinding vb)
        {
            var hasSymbol = TryGetSymbol(name, ExtractVariableIndex, out var sb);
            if (!hasSymbol)
            {
                vb = default;
                return false;
            }

            vb = new VariableBinding()
            {
                type = _scopeStack[sb.ContextIndex].GetVariable(sb.CodeIndex).Type,
                binding = sb
            };
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ExtractVariableIndex(string name, SymbolScope scope)
        {
            if (scope.IsVarDefined(name))
            {
                return scope.GetVariableNumber(name);
            }
            else
                return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ExtractMethodIndex(string name, SymbolScope scope)
        {
            if (scope.IsMethodDefined(name))
            {
                return scope.GetMethodNumber(name);
            }
            else
                return -1;
        }

        public SymbolScope GetScope(int scopeIndex)
        {
            return _scopeStack[scopeIndex];
        }

        public int ScopeIndex(SymbolScope scope)
        {
            return _scopeStack.IndexOf(scope);
        }

        public SymbolBinding DefineMethod(BslMethodInfo method)
        {
            if (_scopeStack.Count > 0)
            {
                if (!HasSymbol(x => x.IsMethodDefined(method.Name)))
                {
                    var idx = TopIndex();
                    var num = _scopeStack[TopIndex()].DefineMethod(method);
                    return new SymbolBinding()
                    {
                        ContextIndex = idx,
                        CodeIndex = num
                    };
                }
                else
                    throw new CompilerException("Symbol already defined");
            }

            throw new InvalidOperationException("Scopes are not defined");
        }

        public SymbolBinding DefineVariable(string name, string alias = null)
        {
            if (_scopeStack.Count > 0)
            {
                var idx = TopIndex();
                var scope = GetScope(idx);
                if (!scope.IsVarDefined(name))
                {
                    var num = scope.DefineVariable(name, alias);
                    return new SymbolBinding()
                    {
                        ContextIndex = idx,
                        CodeIndex = num
                    };
                }
                else
                    throw new CompilerException("Symbol already defined");
            }

            throw new InvalidOperationException("Scopes are not defined");
        }

        public SymbolBinding DefineProperty(string name, string alias = null)
        {
            if (_scopeStack.Count > 0)
            {
                var idx = TopIndex();
                var num = _scopeStack[idx].DefineProperty(name, alias);
                return new SymbolBinding()
                {
                    ContextIndex = idx,
                    CodeIndex = num
                };
            }

            throw new InvalidOperationException("Scopes are not defined");
        }

        public int TopIndex()
        {
            return _scopeStack.Count - 1;
        }

    }
}
