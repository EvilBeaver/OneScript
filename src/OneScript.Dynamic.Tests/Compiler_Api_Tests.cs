/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using OneScript.Commons;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Compiler;
using OneScript.Values;
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
            private ISourceCodeIndexer _codeIndexer;
            private BslSyntaxNode _module;

            public BslSyntaxNode Parse(string code)
            {
                var lexer = new DefaultLexer();
                lexer.Iterator = new SourceCodeIterator(code);
                _codeIndexer = lexer.Iterator;
           
                var parser = new DefaultBslParser(lexer, new DefaultAstBuilder(), _errors, new PreprocessorHandlers());

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
                return compiler.Compile(new ModuleInformation()
                {
                    CodeIndexer = _codeIndexer,
                    Origin = "<text>",
                    ModuleName = "<test>"
                }, _module, scopes, null);
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