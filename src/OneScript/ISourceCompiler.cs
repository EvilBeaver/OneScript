using System;
namespace OneScript
{
    public interface ISourceCompiler
    {
        ICompiledModule Compile(IScriptSource moduleSource);
        
        void InjectObject(Core.IRuntimeContextInstance context);
        
        void InjectSymbol(string name, Core.IValue value);
        
        PreprocessorDirectivesSet PreprocessorDirectives { get; }
    }
}
