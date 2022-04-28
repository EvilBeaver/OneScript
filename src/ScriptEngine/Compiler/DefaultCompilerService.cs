/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.DependencyInjection;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Compiler;
using OneScript.Sources;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    public class DefaultCompilerService : CompilerServiceBase
    {
        private readonly CompilerOptions _сompilerOptions;
        private readonly IServiceContainer _serviceContainer;
        private readonly CompilerBackendSelector _backendSelector;

        public DefaultCompilerService(
            CompilerOptions сompilerOptions,
            ICompilerContext outerContext,
            IServiceContainer serviceContainer) 
            : base(outerContext)
        {
            _сompilerOptions = сompilerOptions;
            _serviceContainer = serviceContainer;
            _backendSelector = new CompilerBackendSelector();

            _backendSelector.NativeBackendInitializer = NativeInitializer;
            _backendSelector.StackBackendInitializer = StackInitializer;

        }

        private StackMachineCodeGenerator StackInitializer()
        {
            var codeGen = new StackMachineCodeGenerator(_сompilerOptions.ErrorSink);
            codeGen.ProduceExtraCode = GetCodeFlags();
            codeGen.DependencyResolver = _сompilerOptions.DependencyResolver;

            return codeGen;
        }

        private ModuleCompiler NativeInitializer()
        {
            var codeGen = new ModuleCompiler(_сompilerOptions.ErrorSink, _serviceContainer);
            return codeGen;
        }

        protected override IExecutableModule CompileInternal(SourceCode source, IEnumerable<string> preprocessorConstants, ICompilerContext context)
        {
            var handlers = _сompilerOptions.PreprocessorHandlers;
            var lexer = CreatePreprocessor(source, preprocessorConstants, handlers);
            var moduleNode = ParseSyntaxConstruction(
                lexer,
                handlers,
                source,
                p => p.ParseStatefulModule());

            return BuildModule(context, moduleNode, source);
        }

        protected override IExecutableModule CompileBatchInternal(SourceCode source, IEnumerable<string> preprocessorConstants, ICompilerContext context)
        {
            var handlers = _сompilerOptions.PreprocessorHandlers;
            var lexer = CreatePreprocessor(source, preprocessorConstants, handlers);
            var moduleNode = ParseSyntaxConstruction(
                lexer,
                handlers,
                source,
                p => p.ParseCodeBatch());

            return BuildModule(context, moduleNode, source);
        }

        protected override IExecutableModule CompileExpressionInternal(SourceCode source, ICompilerContext context)
        {
            var handlers = _сompilerOptions.PreprocessorHandlers;
            
            var lexer = new DefaultLexer
            {
                Iterator = source.CreateIterator()
            };
            
            var moduleNode = ParseSyntaxConstruction(
                lexer,
                handlers,
                source,
                p => p.ParseExpression());

            return BuildModule(context, moduleNode, source);
        }
        
        private ModuleNode ParseSyntaxConstruction(
            ILexer lexer,
            PreprocessorHandlers handlers,
            SourceCode source,
            Func<DefaultBslParser, BslSyntaxNode> action)
        {
            var parser = new DefaultBslParser(
                lexer,
                _сompilerOptions.ErrorSink,
                handlers);

            ModuleNode moduleNode;
            
            try
            {
                moduleNode = (ModuleNode) action(parser);
            }
            catch (SyntaxErrorException e)
            {
                if (e.ModuleName == default)
                {
                    e.ModuleName = source.Name;
                }

                throw;
            }

            return moduleNode;
        }

        private StackRuntimeModule BuildModule(ICompilerContext context, ModuleNode moduleNode, SourceCode src)
        {
            var codeGen = _backendSelector.Select(moduleNode); 
            
            /* TODO Вот тут надо сделать биндинг другой,
            не связанный с ICompilerContext и его стековыми структурами
            и тогда поддержать обобщенный вызов бэкенда */
            if (codeGen is StackMachineCodeGenerator stackBackend)
                return stackBackend.CreateModule(moduleNode, src, context);

            throw new NotImplementedException();
        }

        private CodeGenerationFlags GetCodeFlags()
        {
            CodeGenerationFlags cs = CodeGenerationFlags.Always;
            if (GenerateDebugCode)
                cs |= CodeGenerationFlags.DebugCode;

            if (GenerateCodeStat)
                cs |= CodeGenerationFlags.CodeStatistics;

            return cs;
        }

        private PreprocessingLexer CreatePreprocessor(SourceCode source,
            IEnumerable<string> preprocessorConstants,
            PreprocessorHandlers handlers)
        {
            return CreatePreprocessor(source, preprocessorConstants, handlers, _сompilerOptions.ErrorSink);
        }
    }
}