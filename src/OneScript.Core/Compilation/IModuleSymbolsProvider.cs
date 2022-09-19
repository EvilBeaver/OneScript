using System;
using System.Collections.Generic;
using OneScript.Compilation.Binding;

namespace OneScript.Compilation
{
    public interface IModuleSymbolsProvider
    {
        void FillSymbols(SymbolScope moduleScope);
    }
}