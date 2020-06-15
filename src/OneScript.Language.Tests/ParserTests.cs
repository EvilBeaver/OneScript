/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using Xunit;
using FluentAssertions;

namespace OneScript.Language.Tests
{
    public class ParserTests
    {
        [Fact]
        public void CheckBuild_Of_VariablesSection()
        {
            var code = @"
            Перем П1;
            Перем П2 Экспорт;
            &Аннотация
            Перем П3;
            Перем П4 Экспорт, П5 Экспорт;";
            
            var lexer = new Lexer();
            lexer.Code = code;

            var client = new TestParserClient();
            var parser = new DefaultBslParser(client);
            parser.ParseStatefulModule(lexer);

            parser.Errors.Should().BeEmpty(parser.Errors.Describe());
            var treeValidator = new SyntaxTreeValidator(client.RootNode);
            
            treeValidator.Is("ModuleVariables");

            var child = treeValidator.NextChild();
            child.Is("Variable")
                .WithNode("Identifier")
                .Equal("П1");

            child = treeValidator.NextChild();
            child.Is("Variable")
                .WithNode("Identifier")
                .Equal("П2");
            child.HasNode("Export");
            
            child = treeValidator.NextChild();
            child.Is("Variable")
                .WithNode("Annotation")
                .Equal("Аннотация");
            
            child.HasNode("Identifier")
                .Equal("П3");
            
            child = treeValidator.NextChild();
            child.Is("Variable").WithNode("Identifier").Equal("П4");
            child.HasNode("Export");
            
            child = treeValidator.NextChild();
            child.Is("Variable").WithNode("Identifier").Equal("П5");
            child.HasNode("Export");
        }
    }
}