/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using OneScript.Compilation;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Native.Compiler;
using OneScript.Sources;
using Xunit;

namespace OneScript.Dynamic.Tests
{
    public class NativeRuntimeAnnotationHandlerTests
    {
        [Theory]
        [InlineData("#stack\n#native")]
        [InlineData("#native\n#stack")]
        [InlineData("#stack\n#stack")]
        [InlineData("#native\n#native")]
        public void Duplicate_Runtime_Directive_Should_Report_Error(string code)
        {
            var errors = ParseModule(code);

            errors.Should().ContainSingle(e => e.ErrorId == "RuntimeDirectiveAlreadyDefined");
        }

        [Theory]
        [InlineData("#native")]
        [InlineData("#stack")]
        public void Single_Runtime_Directive_Should_Parse(string code)
        {
            var errors = ParseModule(code);

            errors.Should().BeEmpty();
        }

        [Fact]
        public void Duplicate_Runtime_Directive_Should_Throw_CompilerException()
        {
            var code = "#stack\n#native";

            var act = () => ParseModuleWithThrowingSink(code);

            act.Should().Throw<CompilerException>()
                .Which.LineNumber.Should().Be(2);
        }

        private static System.Collections.Generic.IEnumerable<CodeError> ParseModule(string code)
        {
            var errors = new ListErrorSink();
            Parse(code, errors);
            return errors.Errors;
        }

        private static void ParseModuleWithThrowingSink(string code)
        {
            var errors = new ThrowingErrorSink(CompilerException.FromCodeError);
            Parse(code, errors);
        }

        private static void Parse(string code, IErrorSink errors)
        {
            var lexer = new DefaultLexer
            {
                Iterator = SourceCodeBuilder.Create()
                    .FromString(code)
                    .WithName("<text>")
                    .Build()
                    .CreateIterator()
            };

            var handlers = new PreprocessorHandlers(new[] { new NativeRuntimeAnnotationHandler(errors) });
            var preprocessingLexer = new PreprocessingLexer(lexer)
            {
                Handlers = handlers,
                ErrorSink = errors
            };

            var parser = new DefaultBslParser(preprocessingLexer, errors, handlers);
            _ = parser.ParseStatefulModule();
        }
    }
}
