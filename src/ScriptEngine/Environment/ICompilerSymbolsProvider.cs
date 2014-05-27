using System.Collections.Generic;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;

namespace ScriptEngine.Environment
{
    public interface ICompilerSymbolsProvider
    {
        IEnumerable<VariableDescriptor> GetSymbols();
        IEnumerable<MethodInfo> GetMethods();
    }

}
