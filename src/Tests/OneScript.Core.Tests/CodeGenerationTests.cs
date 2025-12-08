/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using Moq;
using OneScript.Compilation.Binding;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Sources;
using ScriptEngine;
using OneScript.Values;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using System.Linq;
using Xunit;

namespace OneScript.Core.Tests
{
    public class CodeGenerationTests
    {
        [Fact]
        public void EmptyImageIsReturnedWithNoCode()
        {
            var code = "";
            var image = BuildModule(code, Mock.Of<IBslProcess>());
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

            var image = BuildModule(code, Mock.Of<IBslProcess>());
            image.Methods.Should().HaveCount(2);
        }
        
        [Fact]
        public void Variables_Are_Registered_In_Image()
        {
            var code = "Var A;\n" +
                       "Var B;";

            var image = BuildModule(code, Mock.Of<IBslProcess>());
            image.Fields.Should().HaveCount(2);
        }
        
        [Fact]
        public void AnnotationsAsValuesInCode() {
            var code = @"
            &Аннотация(Параметр = &ТожеАннотация(&СТожеПараметромАннотацией, П2 = &СТожеПараметромАннотацией))
            Процедура Процедура1() Экспорт
            КонецПроцедуры";
            var image = BuildModule(code, Mock.Of<IBslProcess>());
            image.Should().NotBeNull();
            image.Constants.Should().HaveCount(0);
            image.Methods.Should().HaveCount(1);
            image.Fields.Should().BeEmpty();

            var method = image.Methods[0];
            method.GetAnnotations().Should().HaveCount(1);

            var annotation = method.GetAnnotations()[0];
            annotation.Parameters.Should().HaveCount(1);

            var annotationParameter = annotation.Parameters.First();
            annotationParameter.Value.Should().NotBeNull();
            annotationParameter.Value.Should().BeOfType<BslAnnotationValue>();

            var parameterValue = (BslAnnotationValue)annotationParameter.Value;
            parameterValue.Parameters.Should().HaveCount(2);
            parameterValue.Parameters.ElementAt(0).Value.Should().BeOfType<BslAnnotationValue>();
            parameterValue.Parameters.ElementAt(1).Name.Should().Be("П2");
            parameterValue.Parameters.ElementAt(1).Value.Should().BeOfType<BslAnnotationValue>();
        }


        private static StackRuntimeModule BuildModule(string code, IBslProcess process)
        {
            var lexer = new DefaultLexer();
            lexer.Iterator = SourceCodeBuilder.Create().FromString(code).Build().CreateIterator();
            var errSink = new ThrowingErrorSink();
            var parser = new DefaultBslParser(
                lexer,
                errSink,
                Mock.Of<PreprocessorHandlers>());
            
            var node = parser.ParseStatefulModule() as ModuleNode;

            var ctx = new SymbolTable();
            ctx.PushScope(new SymbolScope(), ScopeBindingDescriptor.Static(null));
            var compiler = new StackMachineCodeGenerator(errSink, ExplicitImportsBehavior.Disabled);
            return compiler.CreateModule(node, lexer.Iterator.Source, ctx, process);
        }
    }
}