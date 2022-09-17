using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Commons;
using OneScript.Compilation.Binding;
using OneScript.DependencyInjection;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Compiler;
using OneScript.Sources;

namespace OneScript.Dynamic.Tests
{
    internal class CompileHelper
    {
        private readonly IServiceContainer _services;
        private readonly IErrorSink _errors = new ListErrorSink();
        private SourceCode _codeIndexer;
        private BslSyntaxNode _module;

        public CompileHelper(IServiceContainer services)
        {
            _services = services;
        }
        
        public CompileHelper()
        {
            _services = default;
        }

        public IEnumerable<CodeError> Errors => _errors.Errors;

        public BslSyntaxNode ParseBatch(string code)
        {
            var parser = GetBslParser(code);

            _module = parser.ParseCodeBatch(true);
            ThrowOnErrors();

            return _module;
        }
            
        public BslSyntaxNode ParseModule(string code)
        {
            var parser = GetBslParser(code);

            _module = parser.ParseStatefulModule();
            ThrowOnErrors();

            return _module;
        }

        public void ThrowOnErrors()
        {
            if (_errors.HasErrors)
            {
                var prefix = Locale.NStr("ru = 'Ошибка комиляции модуля'; en = 'Module compilation error'");
                var text = string.Join('\n',
                    (new[] { prefix }).Concat(_errors.Errors.Select(x => x.ToString(CodeError.ErrorDetails.Simple))));
                throw new Exception(text);
            }
        }

        private DefaultBslParser GetBslParser(string code)
        {
            var lexer = new DefaultLexer();
            lexer.Iterator = SourceCodeBuilder.Create()
                .FromString(code)
                .WithName("<text>")
                .Build()
                .CreateIterator();
            _codeIndexer = lexer.Iterator.Source;

            var parser = new DefaultBslParser(lexer, _errors, new PreprocessorHandlers());
            return parser;
        }

        public DynamicModule Compile(SymbolTable scopes)
        {
            if (scopes.ScopeCount == 0)
                scopes.PushScope(new SymbolScope(), null);
            var compiler = new ModuleCompiler(_errors, _services);
            return compiler.Compile(_codeIndexer, _module, scopes);
        }
    }
}