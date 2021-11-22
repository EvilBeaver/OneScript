/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using OneScript.Sources;

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

        public int DefineMethod(BslMethodInfo methodSignature)
        {
            RegisterScopeIfNeeded();

            return _currentContext.DefineMethod(methodSignature).CodeIndex;
        }

        public void DefinePreprocessorValue(string name)
        {
            _preprocessorVariables.Add(name);
        }

        public IExecutableModule Compile(SourceCode source, Type classType = null)
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

        public IExecutableModule CompileExpression(SourceCode source)
        {
            return CompileExpressionInternal(source, _currentContext);
        }
        
        public IExecutableModule CompileBatch(SourceCode source)
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

        protected abstract StackRuntimeModule CompileInternal(SourceCode source, IEnumerable<string> preprocessorConstants, ICompilerContext context);
        
        protected abstract StackRuntimeModule CompileBatchInternal(SourceCode source, IEnumerable<string> preprocessorConstants, ICompilerContext context);
        
        protected abstract StackRuntimeModule CompileExpressionInternal(SourceCode source, ICompilerContext context);
        

        private void RegisterScopeIfNeeded()
        {
            if (_scope == null)
            {
                _scope = new SymbolScope();
                _currentContext.PushScope(_scope);
            }
        }
    }
}