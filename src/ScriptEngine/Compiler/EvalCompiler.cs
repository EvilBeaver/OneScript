using System;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace ScriptEngine.Compiler
{
    public class EvalCompiler : CompilerFrontendBase
    {
        private readonly DefaultCompilerBackend _backend;

        public EvalCompiler(
            IErrorSink errorSink,
            IServiceContainer services) : base(new PreprocessorHandlers(), errorSink, services)
        {
            _backend = new DefaultCompilerBackend(errorSink);
        }

        protected override IExecutableModule CompileInternal(SymbolTable symbols, ModuleNode parsedModule, Type classType)
        {
            throw new NotSupportedException();
        }

        protected override IExecutableModule CompileExpressionInternal(SymbolTable symbols, ModuleNode parsedModule)
        {
            _backend.Symbols = symbols;
            _backend.GenerateDebugCode = false;
            _backend.GenerateCodeStat = false;
            return _backend.Compile(parsedModule, default);
        }

        protected override IExecutableModule CompileBatchInternal(SymbolTable symbols, ModuleNode parsedModule)
        {
            _backend.Symbols = symbols;
            _backend.GenerateDebugCode = false;
            _backend.GenerateCodeStat = false;
            return _backend.Compile(parsedModule, default);
        }
    }
}