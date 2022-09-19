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
        
        public SymbolTable Symbols { get; set; }

        public ICompileTimeSymbolsProvider ModuleSymbols { get; set; }
        
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
            if (Symbols != default)
            {
                for (int i = 0; i < Symbols.ScopeCount; i++)
                {
                    actualTable.PushScope(Symbols.GetScope(i), actualTable.GetBinding(i));
                }
            }

            if (ModuleSymbols != default)
            {
                var moduleScope = new SymbolScope();
                foreach (var methodSymbol in ModuleSymbols.Methods)
                {
                    moduleScope.DefineMethod(methodSymbol);
                }
                
                foreach (var variableSymbol in ModuleSymbols.Variables)
                {
                    moduleScope.DefineVariable(variableSymbol);
                }

                actualTable.PushScope(moduleScope, null);
            }
            else
            {
                actualTable.PushScope(new SymbolScope(), null);
            }

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