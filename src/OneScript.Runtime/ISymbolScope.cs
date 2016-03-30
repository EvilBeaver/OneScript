using System;
using System.Collections.Generic;

namespace OneScript.Runtime
{
    public interface ISymbolScope
    {
        int GetMethodNumber(string name);
        IEnumerable<string> GetMethodSymbols();
        int GetVariableNumber(string name);
        IEnumerable<string> GetVariableSymbols();
        bool IsMethodDefined(string name);
        bool IsVarDefined(string name);
        int MethodCount { get; }
        int VariableCount { get; }
        int DefineVariable(string name);
        int DefineMethod(string name);
        void SetMethodAlias(int methodNumber, string alias);
        void SetVariableAlias(int variableNumber, string alias);
    }
}
