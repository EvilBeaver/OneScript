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
            var expectedTree = new BinExpressionNode();
            var first2 = new OperandNode() { content = "2" };
            var second2 = new OperandNode() { content = "2" };
            var one = new OperandNode() { content = "1" };

            expectedTree.opCode = Token.Plus;
            var subExpression = new BinExpressionNode();
            subExpression.opCode = Token.Plus;
            subExpression.left = first2;
            subExpression.right = second2;
            expectedTree.left = subExpression;
            expectedTree.right = one;
            
            var builder = new Builder();
            var lexer = new Lexer();
            lexer.Code = "А = 2 + 2 + 1";
            var parser = new Parser(builder);
            Assert.IsTrue(parser.Build(lexer));
            Assert.IsTrue(builder.topNode is BinExpressionNode);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)builder.topNode, expectedTree));

        }

        [TestMethod]
        public void Expression_DifferentPriority()
        {
            var expectedTree = new BinExpressionNode();
            var first2 = new OperandNode() { content = "2" };
            var second2 = new OperandNode() { content = "2" };
            var one = new OperandNode() { content = "1" };

            var subExpression = new BinExpressionNode();
            subExpression.opCode = Token.Multiply;
            subExpression.left = second2;
            subExpression.right = one;
            expectedTree.opCode = Token.Plus;
            expectedTree.left = first2;
            expectedTree.right = subExpression;
            
            var builder = new Builder();
            var lexer = new Lexer();
            lexer.Code = "А = 2 + 2 * 1";
            var parser = new Parser(builder);
            Assert.IsTrue(parser.Build(lexer));
            Assert.IsTrue(builder.topNode is BinExpressionNode);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)builder.topNode, expectedTree));
        }

        [TestMethod]
        public void Expression_Parenthesis_Priority()
        {
            var expectedTree = new BinExpressionNode();
            var first2 = new OperandNode() { content = "2" };
            var second2 = new OperandNode() { content = "2" };
            var one = new OperandNode() { content = "1" };

            var subExpression = new BinExpressionNode();
            subExpression.opCode = Token.Plus;
            subExpression.left = first2;
            subExpression.right = second2;
            expectedTree.opCode = Token.Multiply;
            expectedTree.left = subExpression;
            expectedTree.right = one;


            var builder = new Builder();
            var lexer = new Lexer();
            lexer.Code = "А = (2 + 2) * 1";
            var parser = new Parser(builder);
            Assert.IsTrue(parser.Build(lexer));
            Assert.IsTrue(builder.topNode is BinExpressionNode);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)builder.topNode, expectedTree));

            subExpression = new BinExpressionNode();
            subExpression.opCode = Token.Multiply;
            subExpression.left = second2;
            subExpression.right = one;
            expectedTree.opCode = Token.Plus;
            expectedTree.left = first2;
            expectedTree.right = subExpression;

            lexer.Code = "А = 2 + (2 * 1)";
            Assert.IsTrue(parser.Build(lexer));
            Assert.IsTrue(builder.topNode is BinExpressionNode);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)builder.topNode, expectedTree));
            
        }
        
    }

    abstract class TestASTNodeBase : IASTNode, IEquatable<IASTNode>
    {
        public bool Equals(IASTNode other)
        {
            if(other.GetType() == this.GetType())
            {
                return EqualsInternal(other);
            }
            else
            {
                return false;
            }
        }

        protected abstract bool EqualsInternal(IASTNode other);

        public static bool CompareTrees(TestASTNodeBase actual, TestASTNodeBase expected)
        {
            return expected.Equals(actual);
        }
    }

    class BinExpressionNode : TestASTNodeBase
    {
        public IASTNode left;
        public IASTNode right;
        public Token opCode;

        protected override bool EqualsInternal(IASTNode other)
        {
            var binExpr = other as BinExpressionNode;
            bool lefts = ((IEquatable<IASTNode>)left).Equals(binExpr.left);
            bool rights = ((IEquatable<IASTNode>)right).Equals(binExpr.right);
            return opCode == binExpr.opCode && lefts && rights;
        }
    }

    class UnaryExpressionNode : TestASTNodeBase
    {
        public IASTNode operand;
        public Token opCode;

        protected override bool EqualsInternal(IASTNode other)
        {
            var un = other as UnaryExpressionNode;
            return opCode == un.opCode && ((IEquatable<IASTNode>)operand).Equals(un.operand);
        }
    }

    class OperandNode : TestASTNodeBase
    {
        public string content;
        protected override bool EqualsInternal(IASTNode other)
        {
            return (other as OperandNode).content == content;
        }
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


        public IASTNode UnaryOperation(Token token, IASTNode operandNode)
        {
            throw new NotImplementedException();
        }
    }
}
