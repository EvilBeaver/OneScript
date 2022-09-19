using System.Collections.Generic;
using OneScript.Compilation;
using OneScript.Compilation.Binding;

namespace ScriptEngine.Machine.Contexts
{
    public class CompileTimeSymbols : ICompileTimeSymbolsProvider
    {
        protected CompileTimeSymbols(ICompileTimeSymbolsProvider baseType)
        {
            var vars = new List<IVariableSymbol>(baseType.Variables);
            var meths = new List<IMethodSymbol>(baseType.Methods);
            // ReSharper disable once VirtualMemberCallInConstructor
            FillSymbols(vars, meths);
            
            Variables = vars.AsReadOnly();
            Methods = meths.AsReadOnly();
        }

        protected virtual void FillSymbols(List<IVariableSymbol> vars, List<IMethodSymbol> meths)
        {
        }

        public IReadOnlyList<IVariableSymbol> Variables { get; }
        
        public IReadOnlyList<IMethodSymbol> Methods { get; }
    }
}