using OneScript.Language.SyntaxAnalysis;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine
{
    /// <summary>
    /// Сервис компиляции единичного модуля.
    /// </summary>
    public interface ICompilerService
    {
        CodeGenerationFlags ProduceExtraCode { get; set; }
        
        int DefineVariable(string name, string alias, SymbolType type);
        
        int DefineMethod(MethodInfo methodInfo);
        
        void DefinePreprocessorValue(string name);
        
        ModuleImage Compile(ICodeSource source);

        void AddDirectiveHandler(IDirectiveHandler handler);
        
        void RemoveDirectiveHandler(IDirectiveHandler handler);
    }
}