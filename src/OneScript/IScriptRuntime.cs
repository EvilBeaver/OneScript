using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript
{
    public interface IScriptRuntime
    {
        DataType RegisterType(string name, string alias, DataTypeConstructor constructor = null);

        void InjectObject(IRuntimeContextInstance context);

        void InjectSymbol(string name, IValue value);

        PreprocessorDirectivesSet PreprocessorDirectives { get; }

        IValue Eval(string expression);

        ICompiledModule Compile(IScriptSource moduleSource);

        void Execute(ICompiledModule module, string entryPointName);
    }
}
