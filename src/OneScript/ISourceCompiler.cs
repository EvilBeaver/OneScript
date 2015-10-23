using System;
namespace OneScript
{
    public interface ISourceCompiler
    {
        ILoadedModule Compile(IScriptSource moduleSource);
        void InjectObject(OneScript.Core.IRuntimeContextInstance context);
        void InjectSymbol(string name, OneScript.Core.IValue value);
    }
}
