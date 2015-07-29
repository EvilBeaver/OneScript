/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    interface ICompilerContext
    {
        SymbolBinding DefineMethod(ScriptEngine.Machine.MethodInfo method);
        SymbolBinding DefineProperty(string name);
        SymbolBinding DefineVariable(string name);
        SymbolBinding GetMethod(string name);
        SymbolScope GetScope(int scopeIndex);
        VariableBinding GetVariable(string name);
        SymbolScope Peek();
        SymbolScope PopScope();
        void PushScope(SymbolScope scope);
        int ScopeIndex(SymbolScope scope);
        int TopIndex();
    }
}
