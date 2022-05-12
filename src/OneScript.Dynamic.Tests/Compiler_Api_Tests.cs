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
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Compiler;
using OneScript.Runtime.Binding;
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
        }

        [Fact]
        public void CanCompile_ModuleBody()
        {
            var module = CreateModule("А = 1");
            module.Methods.Should().HaveCount(1);
        }
    
        private class CompileHelper
        {
            private IErrorSink _errors = new ListErrorSink();
            private SourceCode _codeIndexer;
            private BslSyntaxNode _module;

            public BslSyntaxNode Parse(string code)
            {
                var lexer = new DefaultLexer();
                lexer.Iterator = SourceCodeBuilder.Create()
                    .FromString(code)
                    .WithName("<text>")
                    .Build()
                    .CreateIterator();
                _codeIndexer = lexer.Iterator.Source;
           
                var parser = new DefaultBslParser(lexer, _errors, new PreprocessorHandlers());

                _module = parser.ParseCodeBatch(true);
                if (_errors.HasErrors)
                {
                    var prefix = Locale.NStr("ru = 'Ошибка комиляции модуля'; en = 'Module compilation error'");
                    var text = string.Join('\n', (new[] {prefix}).Concat(_errors.Errors.Select(x => x.Description)));
                    throw new Exception(text);
                }

                return _module;
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
            helper.Parse(code);
            return helper.Compile(new SymbolTable());
        }
        
        private DynamicModule CreateModule(string code, SymbolTable scopes)
        {
            var helper = new CompileHelper();
            helper.Parse(code);
            return helper.Compile(scopes);
        }
    }
}