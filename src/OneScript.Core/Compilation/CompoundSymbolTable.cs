/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
using OneScript.Compilation.Binding;
using OneScript.Contexts;

namespace OneScript.Compilation
{
    public class CompoundSymbolTable : ISymbolTable
    {
        private readonly ISymbolTable _masterTable;
        private readonly SymbolTable _innerTable;
        
        public CompoundSymbolTable(ISymbolTable masterTable)
        {
            _masterTable = masterTable;
            _innerTable = new SymbolTable();
        }

        private int MasterSize => _masterTable.ScopeCount;
        
        public SymbolScope GetScope(int index)
        {
            return index < MasterSize ?
                _masterTable.GetScope(index) : 
                _innerTable.GetScope(index - MasterSize);
        }

        public IAttachableContext GetBinding(int scopeIndex)
        {
            return scopeIndex < MasterSize ? 
                _masterTable.GetBinding(scopeIndex) :
                _innerTable.GetBinding(scopeIndex - MasterSize);
        }

        public int ScopeCount => _masterTable.ScopeCount + _innerTable.ScopeCount;

        public int PushScope(SymbolScope scope, IAttachableContext target)
        {
            return _innerTable.PushScope(scope, target) + MasterSize;
        }

        public void PopScope()
        {
            if (_innerTable.ScopeCount == 0)
                throw new InvalidOperationException("Inner scopes are empty");
            
            _innerTable.PopScope();
        }

        public bool FindVariable(string name, out SymbolBinding binding)
        {
            if (_innerTable.FindVariable(name, out binding))
            {
                ShiftToPublicIndex(ref binding);
                return true;
            }
            
            return _masterTable.FindVariable(name, out binding);
        }

        public bool TryFindMethodBinding(string name, out SymbolBinding binding)
        {
            if (_innerTable.TryFindMethodBinding(name, out binding))
            {
                ShiftToPublicIndex(ref binding);
                return true;
            }
            
            return _masterTable.TryFindMethodBinding(name, out binding);
        }

        public bool TryFindMethod(string name, out IMethodSymbol method)
        {
            return _innerTable.TryFindMethod(name, out method) 
                   || _masterTable.TryFindMethod(name, out method);
        }

        public SymbolBinding DefineMethod(IMethodSymbol symbol)
        {
            var binding = _innerTable.DefineMethod(symbol);
            ShiftToPublicIndex(ref binding);
            return binding;
        }

        public SymbolBinding DefineVariable(IVariableSymbol symbol)
        {
            var binding = _innerTable.DefineVariable(symbol);
            ShiftToPublicIndex(ref binding);
            return binding;
        }

        public IVariableSymbol GetVariable(SymbolBinding binding)
        {
            if (binding.ScopeNumber < MasterSize)
            {
                return _masterTable.GetVariable(binding);
            }
            else
            {
                UnshiftToInnerIndex(ref binding);
                return _innerTable.GetVariable(binding);
            }
        }

        public IMethodSymbol GetMethod(SymbolBinding binding)
        {
            if (binding.ScopeNumber < MasterSize)
            {
                return _masterTable.GetMethod(binding);
            }
            else
            {
                UnshiftToInnerIndex(ref binding);
                return _innerTable.GetMethod(binding);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ShiftToPublicIndex(ref SymbolBinding binding)
        {
            binding.ScopeNumber += MasterSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnshiftToInnerIndex(ref SymbolBinding binding)
        {
            binding.ScopeNumber -= MasterSize;
        }
    }
}