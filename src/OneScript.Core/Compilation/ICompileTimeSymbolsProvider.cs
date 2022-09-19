using System;
using System.Collections.Generic;
using OneScript.Compilation.Binding;

namespace OneScript.Compilation
{
    public interface ICompileTimeSymbolsProvider
    {
        IReadOnlyList<IVariableSymbol> Variables { get; }
        
        IReadOnlyList<IMethodSymbol> Methods { get; }
    }
}