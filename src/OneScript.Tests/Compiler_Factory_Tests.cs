using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;
using OneScript.Scripting.ByteCode;

namespace OneScript.Tests
{
    [TestClass]
    public class Compiler_Factory_Tests
    {
        [TestMethod]
        public void LexemExtractor_GetsTokens()
        {
            var code = "Перем А;";
            var lexer = new Lexer();
            lexer.Code = code;

            var extractor = new LexemExtractor(lexer);

            extractor.Next();
            Assert.IsTrue(extractor.LastExtractedLexem.Token == Token.VarDef);

            extractor.Next();
            Assert.IsTrue(extractor.LastExtractedLexem.Token == Token.NotAToken);
            Assert.IsTrue(extractor.LastExtractedLexem.Type == LexemType.Identifier);

            extractor.Next();
            Assert.IsTrue(extractor.LastExtractedLexem.Token == Token.Semicolon);
            Assert.IsTrue(extractor.LastExtractedLexem.Type == LexemType.EndOperator);

            extractor.Next();
            Assert.IsTrue(extractor.LastExtractedLexem.Token == Token.EndOfText);

            TestHelpers.ExceptionThrown(()=>extractor.Next(), typeof(CompilerException));
        }

        [TestMethod]
        public void ByteCodeFactory_Creation()
        {
            ICodeBlockFactory factory = new ByteCodeBlockFactory();
            
            factory.Init(null, null);
            Assert.IsTrue(factory.GetModuleBuilder() is Scripting.ByteCode.ModuleBuilder);
            Assert.IsTrue(factory.GetVarDefinitionBuilder() is Scripting.ByteCode.VariableDefinitionBuilder);
            Assert.IsTrue(factory.GetMethodSectionBuilder() is Scripting.ByteCode.MethodSectionBuilder);
            Assert.IsTrue(factory.GetMethodBuilder() is Scripting.ByteCode.MethodBuilder);
            Assert.IsTrue(factory.GetCodeBatchBuilder() is Scripting.ByteCode.CodeBatchBuilder);
            Assert.IsTrue(factory.GetComplexStatementBuilder() is Scripting.ByteCode.StatementBuilder);
            Assert.IsTrue(factory.GetStatementBuilder() is Scripting.ByteCode.StatementBuilder);
            Assert.IsTrue(factory.GetProcCallBuilder() is Scripting.ByteCode.CallBuilder);
            Assert.IsTrue(factory.GetAssignmentBuilder() is Scripting.ByteCode.AssignmentBuilder);
            Assert.IsTrue(factory.GetLeftExpressionBuilder() is Scripting.ByteCode.AssignableExpressionBuilder);
            Assert.IsTrue(factory.GetRightExpressionBuilder() is Scripting.ByteCode.SourceExpressionBuilder);


        }

        [TestMethod]
        public void Interpreter_Creation()
        {
            var context = new CompilerContext();
            var interpreter = new CodeInterpreter(new ByteCodeBlockFactory());
            interpreter.Context = new CompilerContext();
            interpreter.Code = "Перем А;";

            ModuleImage result = (ModuleImage)interpreter.Run();

            Assert.IsTrue(result.Variables.Count == 1);
            Assert.IsTrue(result.Variables[0].Name == "А");

        }
    }
}
