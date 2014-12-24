using System;
namespace OneScript.Scripting.Runtime
{
    public interface ICompilerContext
    {
        SymbolBinding DefineMethod(string name, MethodSignatureData methodSignature);
        SymbolBinding DefineVariable(string name);
        SymbolBinding GetMethod(string name);
        SymbolBinding GetVariable(string name);
        bool IsMethodDefined(string name);
        bool IsVarDefined(string name);
        bool TryGetVariable(string name, out SymbolBinding binding);
        bool TryGetMethod(string name, out SymbolBinding binding);
        SymbolScope PopScope();
        void PushScope(SymbolScope scope);
        SymbolScope TopScope { get; }
    }
}
