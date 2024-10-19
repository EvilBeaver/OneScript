﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
    /// <summary>
    /// Компилятор вычислимых выражений
    /// </summary>
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
