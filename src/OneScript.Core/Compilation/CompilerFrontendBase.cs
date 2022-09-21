/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Compilation.Binding;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Sources;

namespace OneScript.Compilation
{
    public abstract class CompilerFrontendBase : ICompilerFrontend
    {
        protected CompilerFrontendBase(
            PreprocessorHandlers handlers,
            IErrorSink errorSink,
            IServiceContainer services)
        {
            PreprocessorHandlers = handlers;
            ErrorSink = errorSink;
            Services = services;
        }

        public IErrorSink ErrorSink { get; }
        
        protected IServiceContainer Services { get; }

        private PreprocessorHandlers PreprocessorHandlers { get; }
        
        public bool GenerateDebugCode { get; set; }
        
        public bool GenerateCodeStat { get; set; }

        public IList<string> PreprocessorDefinitions { get; } = new List<string>();
        
        public SymbolTable SharedSymbols { get; set; }

        public SymbolScope FillSymbols(Type type)
        {
            var symbolsProvider = Services.Resolve<CompileTimeSymbolsProvider>();
            var typeSymbols = symbolsProvider.Get(type);
            ModuleSymbols = new SymbolScope();
            typeSymbols.FillSymbols(ModuleSymbols);

            return ModuleSymbols;
        }
        
        private SymbolScope ModuleSymbols { get; set; }
        
        public IExecutableModule Compile(SourceCode source, Type classType = null)
        {
            var lexer = CreatePreprocessor(source);
            var symbols = PrepareSymbols();
            var parsedModule = ParseSyntaxConstruction(lexer, source, p => p.ParseStatefulModule());

            return CompileInternal(symbols, parsedModule, classType);
        }

        public IExecutableModule CompileExpression(SourceCode source)
        {
            var lexer = new DefaultLexer
            {
                Iterator = source.CreateIterator()
            };
            var symbols = PrepareSymbols();
            var parsedModule = ParseSyntaxConstruction(lexer, source, p => p.ParseExpression());

            return CompileExpressionInternal(symbols, parsedModule);
        }

        public IExecutableModule CompileBatch(SourceCode source)
        {
            var lexer = CreatePreprocessor(source);
            var symbols = PrepareSymbols();
            var parsedModule = ParseSyntaxConstruction(lexer, source, p => p.ParseStatefulModule());

            return CompileBatchInternal(symbols, parsedModule);
        }

        protected abstract IExecutableModule CompileInternal(SymbolTable symbols, ModuleNode parsedModule, Type classType);
        
        protected abstract IExecutableModule CompileExpressionInternal(SymbolTable symbols, ModuleNode parsedModule);
        
        protected abstract IExecutableModule CompileBatchInternal(SymbolTable symbols, ModuleNode parsedModule);

        private SymbolTable PrepareSymbols()
        {
            var actualTable = new SymbolTable();
            if (SharedSymbols != default)
            {
                for (int i = 0; i < SharedSymbols.ScopeCount; i++)
                {
                    actualTable.PushScope(SharedSymbols.GetScope(i), SharedSymbols.GetBinding(i));
                }
            }

            actualTable.PushScope(ModuleSymbols ?? new SymbolScope(), null);

            return actualTable;
        }
        
        private ModuleNode ParseSyntaxConstruction(
            ILexer lexer,
            SourceCode source,
            Func<DefaultBslParser, BslSyntaxNode> action)
        {
            var parser = new DefaultBslParser(
                lexer,
                ErrorSink,
                PreprocessorHandlers);

            ModuleNode moduleNode;
            
            try
            {
                moduleNode = (ModuleNode) action(parser);
            }
            catch (SyntaxErrorException e)
            {
                e.ModuleName ??= source.Name;
                throw;
            }

            return moduleNode;
        }
        
        private ILexer CreatePreprocessor(
            SourceCode source)
        {
            var baseLexer = new DefaultLexer
            {
                Iterator = source.CreateIterator()
            };

            var conditionals = PreprocessorHandlers?.Get<ConditionalDirectiveHandler>();
            if (conditionals != default)
            {
                foreach (var constant in PreprocessorDefinitions)
                {
                    conditionals.Define(constant);
                }
            }

            var lexer = new PreprocessingLexer(baseLexer)
            {
                Handlers = PreprocessorHandlers,
                ErrorSink = ErrorSink
            };
            return lexer;
        }
    }
}
