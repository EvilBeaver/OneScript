using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript
{
    public abstract class ScriptEngine
    {
        abstract public void InjectSymbol(string name, IValue value);

        abstract public void InjectObject(IRuntimeContextInstance context);

        abstract public ILoadedModule CurrentModule { get; }

        abstract public IValue Eval(string expression);

        abstract public void Execute(ILoadedModule module);
        
        public static ScriptEngine Current { get; set; }

    }
}
