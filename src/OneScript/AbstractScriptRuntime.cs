using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript
{
    public abstract class AbstractScriptRuntime
    {
        public AbstractScriptRuntime()
        {
            PreprocessorDirectives = new PreprocessorDirectivesSet();
        }

        abstract public void InjectSymbol(string name, IValue value);

        abstract public void InjectObject(IRuntimeContextInstance context);

        abstract public DataType RegisterType(string name, string alias, DataTypeConstructor constructor = null);

        abstract public IValue Eval(string expression);

        abstract public ILoadedModule Compile(IScriptSource moduleSource);

        abstract public void Execute(ILoadedModule module, string entryPointName);

        public PreprocessorDirectivesSet PreprocessorDirectives { get; protected set; }
    }
}
