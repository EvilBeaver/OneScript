/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    [Obsolete("Use OneScript.Compilation.Binding.SymbolTable")]
    public interface ICompilerContext
    {
        SymbolBinding DefineMethod(BslMethodInfo method);
        SymbolBinding DefineVariable(string name, string alias = null);
        SymbolBinding GetMethod(string name);
        SymbolScope GetScope(int scopeIndex);
        VariableBinding GetVariable(string name);
        bool TryGetVariable(string name, out VariableBinding binding);
        bool TryGetMethod(string name, out SymbolBinding binding);
        SymbolScope PopScope();
        void PushScope(SymbolScope scope);
        int TopIndex();
    }
}
