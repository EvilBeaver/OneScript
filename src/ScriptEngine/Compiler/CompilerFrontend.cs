/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Compiler
{
    /// <summary>
    /// Часть компилятора, независимая от рантайма (нативного или стекового)
    /// Запускает компиляцию модуля, строит AST и делегирует построение кода в бэкенд компилятора под конкретный рантайм.
    /// </summary>
    public class CompilerFrontend : CompilerFrontendBase
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly PredefinedInterfaceResolver _interfaceResolver;
        private readonly CompilerBackendSelector _backendSelector;

        public CompilerFrontend(
            PreprocessorHandlers handlers,
            IErrorSink errorSink,
            IServiceContainer services,
            IDependencyResolver dependencyResolver,
            PredefinedInterfaceResolver interfaceResolver,
            IEnumerable<IPredefinedInterfaceChecker> checkers) : base(handlers, errorSink, services)
        {
            _dependencyResolver = dependencyResolver;
            _interfaceResolver = interfaceResolver;

            _backendSelector = services.Resolve<CompilerBackendSelector>();

            _backendSelector.NativeBackendInitializer = NativeInitializer;
            _backendSelector.StackBackendInitializer = StackInitializer;
        }

        private ICompilerBackend StackInitializer()
        {
            var backend = new DefaultCompilerBackend(ErrorSink);
            SetDefaultOptions(backend);
            backend.DependencyResolver = _dependencyResolver;

            return backend;
        }

        private ICompilerBackend NativeInitializer()
        {
            var backend = new NativeCompilerBackend(ErrorSink, Services);
            SetDefaultOptions(backend);

            return backend;
        }

        private void SetDefaultOptions(ICompilerBackend backend)
        {
            backend.GenerateCodeStat = GenerateCodeStat;
            backend.GenerateDebugCode = GenerateDebugCode;
        }

        protected override IExecutableModule CompileInternal(SymbolTable symbols, ModuleNode parsedModule, Type classType)
        {
            var backend = _backendSelector.Select(parsedModule);
            backend.Symbols = symbols;
            
            var module = backend.Compile(parsedModule, classType);

            _interfaceResolver.Resolve(module);

            return module;
        }

        protected override IExecutableModule CompileExpressionInternal(SymbolTable symbols, ModuleNode parsedModule)
        {
            var backend = _backendSelector.Select(parsedModule);
            backend.Symbols = symbols;
            return backend.Compile(parsedModule, typeof(UserScriptContextInstance));
        }

        protected override IExecutableModule CompileBatchInternal(SymbolTable symbols, ModuleNode parsedModule)
        {
            var backend = _backendSelector.Select(parsedModule);
            backend.Symbols = symbols;
            return backend.Compile(parsedModule, typeof(UserScriptContextInstance));
        }
    }
}
