/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
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
                RegisterScopeIfNeeded();
                return CompileInternal(source, _preprocessorVariables, _currentContext);
            }
            finally
            {
                _currentContext.PopScope();
                _scope = null;
            }
        }

        public ModuleImage CompileExpression(ICodeSource source)
        {
            return CompileExpressionInternal(source, _currentContext);
        }

        public ModuleImage CompileBatch(ICodeSource source)
        {
            try
            {
                RegisterScopeIfNeeded();
                return CompileBatchInternal(source, _preprocessorVariables, _currentContext);
            }
            finally
            {
                _currentContext.PopScope();
                _scope = null;
            }
        }

        protected abstract ModuleImage CompileInternal(ICodeSource source, IEnumerable<string> preprocessorConstants, ICompilerContext context);
        
        protected abstract ModuleImage CompileBatchInternal(ICodeSource source, IEnumerable<string> preprocessorConstants, ICompilerContext context);
        
        protected abstract ModuleImage CompileExpressionInternal(ICodeSource source, ICompilerContext context);
        

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