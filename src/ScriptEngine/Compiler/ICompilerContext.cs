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
