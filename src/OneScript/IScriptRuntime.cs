using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript
{
    public interface IScriptRuntime : ISourceCompiler
    {
        DataType RegisterType(string name, string alias, DataTypeConstructor constructor = null);

        IValue Eval(string expression);

        void Execute(ICompiledModule module, string entryPointName);

    }
}
