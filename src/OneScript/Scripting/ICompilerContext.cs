using System;
namespace OneScript.Scripting
{
    interface ICompilerContext
    {
        SymbolBinding DefineMethod(string name, MethodSignatureData methodSignature);
        SymbolBinding DefineVariable(string name);
        SymbolBinding GetMethod(string name);
        SymbolBinding GetVariable(string name);
        bool IsMethodDefined(string name);
        bool IsVarDefined(string name);
        SymbolScope PopScope();
        void PushScope(SymbolScope scope);
        SymbolScope TopScope { get; }
    }
}
