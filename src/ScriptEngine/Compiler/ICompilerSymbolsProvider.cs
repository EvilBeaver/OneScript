using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    interface ICompilerSymbolsProvider
    {
        IEnumerable<VariableDescriptor> GetSymbols();
        IEnumerable<MethodInfo> GetMethods();
    }

}
