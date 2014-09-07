using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;

namespace OneScript.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Expression_Straight_Priority()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            lexer.Code = "А = 2 + 2 + 1";
            var parser = new Parser(builder);
            Assert.IsTrue(parser.Build(lexer));
            Assert.IsTrue(builder.topNode is BinExpressionNode);

            var binNode = builder.topNode as BinExpressionNode;
            Assert.IsTrue(binNode.opCode == Token.Plus);
            Assert.IsTrue(binNode.left is BinExpressionNode);
            Assert.IsTrue((binNode.right as OperandNode).content == "1");

            binNode = binNode.left as BinExpressionNode;
            Assert.IsTrue(binNode.opCode == Token.Plus);
            Assert.IsTrue((binNode.right as OperandNode).content == "2");
            Assert.IsTrue((binNode.right as OperandNode).content == "2");

        }

        [TestMethod]
        public void Expression_DifferentPriority()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            lexer.Code = "А = 2 + 2 * 1";
            var parser = new Parser(builder);
            Assert.IsTrue(parser.Build(lexer));
            Assert.IsTrue(builder.topNode is BinExpressionNode);

            var binNode = builder.topNode as BinExpressionNode;
            Assert.IsTrue(binNode.opCode == Token.Plus);
            Assert.IsTrue(binNode.left is OperandNode);
            Assert.IsTrue((binNode.left as OperandNode).content == "2");

            binNode = binNode.right as BinExpressionNode;
            Assert.IsTrue(binNode.opCode == Token.Multiply);
            Assert.IsTrue((binNode.left as OperandNode).content == "2");
            Assert.IsTrue((binNode.right as OperandNode).content == "1");
        }
    }

    class BinExpressionNode : IASTNode
    {
        public IASTNode left;
        public IASTNode right;
        public Token opCode;
    }

    class OperandNode : IASTNode
    {
        public string content;
    }

    class Builder : IModuleBuilder
    {

        public IASTNode topNode = null;

        public void BeginModule()
        {
            //throw new NotImplementedException();
        }

        public void CompleteModule()
        {
            //throw new NotImplementedException();
        }

        public void OnError(CompilerErrorEventArgs eventArgs)
        {
            //throw new NotImplementedException();
        }

        public void DefineExportVariable(string symbolicName)
        {
            //throw new NotImplementedException();
        }

        public void DefineVariable(string symbolicName)
        {
            //throw new NotImplementedException();
        }

        public IASTNode SelectOrUseVariable(string identifier)
        {
            return null;
        }

        public void BuildAssignment(IASTNode acceptor, IASTNode source)
        {
            topNode = source;
        }

        public IASTNode ReadLiteral(Lexem lexem)
        {
            return new OperandNode()
            {
                content = lexem.Content
            };
        }

        public IASTNode ReadVariable(string identifier)
        {
            return new OperandNode()
            {
                content = identifier
            };
        }

        public IASTNode BinaryOperation(Token operationToken, IASTNode leftHandedNode, IASTNode rightHandedNode)
        {
            return new BinExpressionNode()
            {
                opCode = operationToken,
                left = leftHandedNode,
                right = rightHandedNode
            };
        }
    }
}
