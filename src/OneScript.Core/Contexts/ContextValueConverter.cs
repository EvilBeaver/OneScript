using System;
using ScriptEngine.Machine;

namespace OneScript.Contexts
{
    public abstract class ContextValueConverter<TClr>
    {
        public abstract IValue ToIValue(TClr obj);

        public abstract TClr ToClr(IValue obj);
    }
}