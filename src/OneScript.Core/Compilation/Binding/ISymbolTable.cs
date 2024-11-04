/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;

namespace OneScript.Compilation.Binding
{
    public interface ISymbolTable
    {
        SymbolScope GetScope(int index);
        IAttachableContext GetBinding(int scopeIndex);
        int ScopeCount { get; }
        int PushScope(SymbolScope scope, IAttachableContext target);
        void PopScope();
        bool FindVariable(string name, out SymbolBinding binding);
        bool TryFindMethodBinding(string name, out SymbolBinding binding);
        bool TryFindMethod(string name, out IMethodSymbol method);
        SymbolBinding DefineMethod(IMethodSymbol symbol);
        SymbolBinding DefineVariable(IVariableSymbol symbol);
        IVariableSymbol GetVariable(SymbolBinding binding);
        IMethodSymbol GetMethod(SymbolBinding binding);
    }
}