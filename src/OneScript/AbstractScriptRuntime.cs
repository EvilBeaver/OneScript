using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript
{
    public abstract class AbstractScriptRuntime : ISourceCompiler
    {
        public AbstractScriptRuntime()
        {
            PreprocessorDirectives = new PreprocessorDirectivesSet();
        }

        abstract public void InjectSymbol(string name, IValue value);

        abstract public void InjectObject(IRuntimeContextInstance context);

        abstract public DataType RegisterType(string name, string alias, DataTypeConstructor constructor = null);

        abstract public IValue Eval(string expression);

        abstract public ICompiledModule Compile(IScriptSource moduleSource);

        abstract public void Execute(ICompiledModule module, string entryPointName);

        abstract public ISourceCompiler CreateCompiler();

        public PreprocessorDirectivesSet PreprocessorDirectives { get; protected set; }
    }
}
