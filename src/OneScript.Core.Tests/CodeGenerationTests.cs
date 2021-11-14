/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using Moq;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Sources;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class CodeGenerationTests
    {
        [Fact]
        public void EmptyImageIsReturnedWithNoCode()
        {
            var code = "";
            var image = BuildModule(code);
            image.Should().NotBeNull();
            image.Code.Should().BeEmpty();
            image.Constants.Should().BeEmpty();
            image.Methods.Should().BeEmpty();
            image.Fields.Should().BeEmpty();
        }

        [Fact]
        public void Methods_Are_Registered_In_Image()
        {
            var code = "Procedure Foo() EndProcedure\n" +
                       "Function Bar() EndFunction";

            var image = BuildModule(code);
            image.Methods.Should().HaveCount(2);
        }
        
        [Fact]
        public void Variables_Are_Registered_In_Image()
        {
            var code = "Var A;\n" +
                       "Var B;";

            var image = BuildModule(code);
            image.Fields.Should().HaveCount(2);
        }

        private static LoadedModule BuildModule(string code)
        {
            var lexer = new DefaultLexer();
            lexer.Iterator = SourceCodeBuilder.Create().FromString(code).Build().CreateIterator();
            var parser = new DefaultBslParser(
                lexer,
                Mock.Of<IErrorSink>(),
                Mock.Of<PreprocessorHandlers>());
            
            var node = parser.ParseStatefulModule() as ModuleNode;

            var ctx = new CompilerContext();
            ctx.PushScope(new SymbolScope());
            var compiler = new StackMachineCodeGenerator(ctx);
            return compiler.CreateModule(node, lexer.Iterator.Source);
        }
    }
}