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
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.Compiler.ByteCode;
using ScriptEngine.Environment;
using Xunit;

namespace OneScript.Core.Tests
{
    public class CodeGenerationTests
    {
        [Fact]
        public void EmptyImageIsReturnedWithNoCode()
        {
            var code = "";
            var image = BuildImage(code);
            image.Should().NotBeNull();
            image.Code.Should().BeEmpty();
            image.Constants.Should().BeEmpty();
            image.Methods.Should().BeEmpty();
            image.Variables.Should().BeEmpty();
        }

        [Fact]
        public void Methods_Are_Registered_In_Image()
        {
            var code = "Procedure Foo() EndProcedure\n" +
                       "Function Bar() EndFunction";

            var image = BuildImage(code);
            image.Methods.Should().HaveCount(2);
        }
        
        [Fact]
        public void Variables_Are_Registered_In_Image()
        {
            var code = "Var A;\n" +
                       "Var B;";

            var image = BuildImage(code);
            image.Variables.Should().HaveCount(2);
        }

        private static ModuleImage BuildImage(string code)
        {
            var parser = DefaultBslParser.PrepareParser(code);
            var node = parser.ParseStatefulModule() as BslSyntaxNode;

            var compiler = new AstBasedCodeGenerator(Mock.Of<ICompilerContext>());
            return compiler.CreateImage(node, new ModuleInformation());
        }
    }
}