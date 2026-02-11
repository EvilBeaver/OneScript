/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FluentAssertions;
using MessagePack;
using Moq;
using Xunit;
using OneScript.Compilation.Binding;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Sources;
using OneScript.Values;
using OneScript.Contexts;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Serialization;
using ScriptEngine.Serialization;

namespace OneScript.Core.Tests
{
    public class ModuleSerializationTests
    {
        [Fact]
        public void Should_Serialize_And_Deserialize_Empty_Module()
        {
            var code = "";
            var environment = new RuntimeEnvironment();
            var original = BuildModule(code, environment, out var symbolTable);
            
            var serializer = new ModuleSerializer();
            var data = serializer.Serialize(original, symbolTable);
            
            var deserializer = new ModuleDeserializer();
            var restored = deserializer.Deserialize(data, environment);
            
            restored.Should().NotBeNull();
            restored.Code.Should().BeEmpty();
            restored.Constants.Should().BeEmpty();
            restored.Methods.Should().BeEmpty();
            restored.Fields.Should().BeEmpty();
        }

        [Fact]
        public void Should_Serialize_And_Deserialize_Module_With_Content()
        {
            var code = "Var A Export;\n" +
                       "Procedure Foo(P1, P2 = 1) Export\n" +
                       "  A = P1 + P2;\n" +
                       "EndProcedure";
            
            var environment = new RuntimeEnvironment();
            var original = BuildModule(code, environment, out var symbolTable);
            
            var serializer = new ModuleSerializer();
            var data = serializer.Serialize(original, symbolTable);
            
            var deserializer = new ModuleDeserializer();
            var restored = deserializer.Deserialize(data, environment);
            
            restored.Should().NotBeNull();
            restored.Methods.Should().HaveCount(original.Methods.Count);
            restored.Fields.Should().HaveCount(original.Fields.Count);
            
            var originalMethod = original.Methods[0];
            var restoredMethod = restored.Methods[0];
            
            restoredMethod.Name.Should().Be(originalMethod.Name);
        }

        [Fact]
        public void Should_Serialize_Module_To_ByteArray()
        {
            var code = "Var A;\n" +
                       "Procedure Test() Export\n" +
                       "EndProcedure";
            
            var environment = new RuntimeEnvironment();
            var original = BuildModule(code, environment, out var symbolTable);
            
            var serializer = new ModuleSerializer();
            var data = serializer.Serialize(original, symbolTable);
            
            data.Should().NotBeNull();
            data.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Should_Deserialize_Module_From_ByteArray()
        {
            var code = "Var A;\n" +
                       "Procedure Test() Export\n" +
                       "EndProcedure";
            
            var environment = new RuntimeEnvironment();
            var original = BuildModule(code, environment, out var symbolTable);
            
            var serializer = new ModuleSerializer();
            var data = serializer.Serialize(original, symbolTable);
            
            var deserializer = new ModuleDeserializer();
            var restored = deserializer.Deserialize(data, environment);
            
            restored.Should().NotBeNull();
            restored.Methods.Should().HaveCount(1);
            restored.Fields.Should().HaveCount(1);
        }

        [Fact]
        public void Should_Serialize_And_Deserialize_SourceInfo_And_Dependencies()
        {
            var code = "Var A;\n" +
                       "Procedure Test() Export\n" +
                       "EndProcedure";

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, code);

            try
            {
                var source = SourceCodeBuilder.Create()
                    .FromSource(new FileCodeSource(tempFile))
                    .WithName("TestModule")
                    .Build();

                var environment = new RuntimeEnvironment();
                var original = BuildModule(source, environment, out var symbolTable);

                var providers = new ICodeSourceImageProvider[] { new FileCodeSourceImageProvider() };
                var imageSerializer = new CodeSourceImageSerializer(providers);
                var dependencies = new[] { "lib-one", "lib-two" };

                var serializer = new ModuleSerializer(imageSerializer);
                var data = serializer.Serialize(original, symbolTable, source, dependencies);

                var deserializer = new ModuleDeserializer(imageSerializer);
                var restored = deserializer.Deserialize(data, environment);
                var image = MessagePackSerializer.Deserialize<ModuleImage>(data);

                restored.Source.Should().NotBeNull();
                restored.Source.Name.Should().Be("TestModule");
                restored.Source.Location.Should().Be(Path.GetFullPath(tempFile));
                image.Dependencies.Should().BeEquivalentTo(dependencies);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        private static StackRuntimeModule BuildModule(string code, IRuntimeEnvironment environment, out SymbolTable symbolTable)
        {
            var source = SourceCodeBuilder.Create().FromString(code).Build();
            return BuildModule(source, environment, out symbolTable);
        }

        private static StackRuntimeModule BuildModule(SourceCode source, IRuntimeEnvironment environment, out SymbolTable symbolTable)
        {
            var lexer = new DefaultLexer();
            lexer.Iterator = source.CreateIterator();
            var errSink = new ThrowingErrorSink();
            var parser = new DefaultBslParser(
                lexer,
                errSink,
                Mock.Of<PreprocessorHandlers>());
            
            var node = parser.ParseStatefulModule() as ModuleNode;
            node.Should().NotBeNull();

            var compiler = new StackMachineCodeGenerator(errSink, ExplicitImportsBehavior.Disabled);
            symbolTable = environment.GetSymbolTable();
            
            // Add ThisScope for the module being compiled
            symbolTable.PushScope(new SymbolScope(), ScopeBindingDescriptor.ThisScope());
            
            return compiler.CreateModule(node, source, symbolTable, Mock.Of<IBslProcess>());
        }

        private class ThrowingErrorSink : IErrorSink
        {
            public void AddError(CodeError error)
            {
                throw new InvalidOperationException(error.Description);
            }

            public IEnumerable<CodeError> Errors => Enumerable.Empty<CodeError>();
            public bool HasErrors => false;
        }
    }
}
