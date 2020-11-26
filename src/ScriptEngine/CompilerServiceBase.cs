using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine
{
    public abstract class CompilerServiceBase : ICompilerService
    {
        private SymbolScope _scope;
        private readonly ModuleCompilerContext _currentContext;
        private readonly PreprocessorHandlers _handlers = new PreprocessorHandlers();
        private readonly List<string> _preprocessorVariables = new List<string>();

        protected CompilerServiceBase(ICompilerContext outerContext)
        {
            _currentContext = new ModuleCompilerContext(outerContext);
        }
        
        public CodeGenerationFlags ProduceExtraCode { get; set; }
        
        public int DefineVariable(string name, string alias, SymbolType type)
        {
            RegisterScopeIfNeeded();

            try
            {
                int varIdx;
                if (type == SymbolType.Variable)
                    varIdx = _currentContext.DefineVariable(name, alias).CodeIndex;
                else
                    varIdx = _currentContext.DefineProperty(name, alias).CodeIndex;

                return varIdx;
            }
            catch
            {
                _currentContext.PopScope();
                _scope = null;
                throw;
            }
        }

        public int DefineMethod(MethodInfo methodInfo)
        {
            RegisterScopeIfNeeded();

            return _currentContext.DefineMethod(methodInfo).CodeIndex;
        }

        public void DefinePreprocessorValue(string name)
        {
            _preprocessorVariables.Add(name);
        }

        public ModuleImage Compile(ICodeSource source)
        {
            try
            {
                return CompileInternal(source, _preprocessorVariables, _currentContext);
            }
            finally
            {
                _currentContext.PopScope();
                _scope = null;
            }
        }

        protected abstract ModuleImage CompileInternal(ICodeSource source, IEnumerable<string> preprocessorConstants, ICompilerContext context);

        public void AddDirectiveHandler(IDirectiveHandler handler)
        {
            _handlers.Add(handler);
        }

        public void RemoveDirectiveHandler(IDirectiveHandler handler)
        {
            _handlers.Remove(handler);
        }
        
        private void RegisterScopeIfNeeded()
        {
            if (_scope == null)
            {
                _scope = new SymbolScope();
                _currentContext.PushScope(_scope);
            }
        }
        
        protected static ModuleInformation CreateModuleInformation(ICodeSource source, ILexer parser)
        {
            var mi = new ModuleInformation();
            mi.CodeIndexer = parser.Iterator;
            // пока у модулей нет собственных имен, будет совпадать с источником модуля
            mi.ModuleName = source.SourceDescription;
            mi.Origin = source.SourceDescription;
            return mi;
        }
    }
}