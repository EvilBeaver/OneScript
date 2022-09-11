/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using FluentAssertions;
using OneScript.Commons;
using OneScript.Compilation.Binding;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Compiler;
using OneScript.Sources;
using Xunit;

namespace OneScript.Dynamic.Tests
{
    public class Compiler_Api_Tests
    {
        [Fact]
        public void CanCompile_Empty_Module()
        {
            var module = CreateModule("");
            Assert.Empty(module.Fields);
            Assert.Empty(module.Methods);
            module.ModuleBody.Should().BeNull();
        }

        [Fact]
        public void CanCompile_ModuleBody()
        {
            var module = CreateModule("А = 1");
            module.Methods.Should().HaveCount(1);
        }

        [Fact]
        public void CanCompile_Module_With_SeveralMethods()
        {
            var module = CreateModule(
                @"Процедура А()
                    Б = 1;
                КонецПроцедуры

                Процедура Б()
                    Б = 1;
                КонецПроцедуры

                В = 4;
                ");

            module.Methods.Should().HaveCount(3);
            module.ModuleBody.Should().NotBeNull();
        }
        
        [Fact]
        public void CanCompile_Module_With_AllSections()
        {
            var module = CreateModule(
                @"Перем Ы;

                Процедура А()
                    Б = 1;
                КонецПроцедуры

                Процедура Б()
                    Б = 1;
                КонецПроцедуры

                В = 4;
                ");
            
            module.Methods.Should().HaveCount(3);
            module.ModuleBody.Should().NotBeNull();
            module.Fields.Should().HaveCount(1);
        }
        
        private class CompileHelper
        {
            private IErrorSink _errors = new ListErrorSink();
            private SourceCode _codeIndexer;
            private BslSyntaxNode _module;

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

            private void ThrowOnErrors()
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
                var compiler = new ModuleCompiler(_errors, null);
                return compiler.Compile(_codeIndexer, _module, scopes);
            }
        }
        
        private DynamicModule CreateModule(string code)
        {
            var helper = new CompileHelper();
            helper.ParseModule(code);
            return helper.Compile(new SymbolTable());
        }
        
        private DynamicModule CreateModule(string code, SymbolTable scopes)
        {
            var helper = new CompileHelper();
            helper.ParseBatch(code);
            return helper.Compile(scopes);
        }
    }
}