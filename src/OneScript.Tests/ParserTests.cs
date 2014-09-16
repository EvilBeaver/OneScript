using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;
using System.Collections.Generic;

namespace OneScript.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void TopLevel_Semicolons()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);

            lexer.Code = ";; А = 1;;";
            Assert.IsTrue(parser.Build(lexer));
            Assert.IsTrue(builder.topNode is AssignmentNode);
            Assert.IsTrue((builder.topNode as AssignmentNode).Right is OperandNode);
        }

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
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expectedTree));

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
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expectedTree));
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
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expectedTree));

            subExpression = new BinExpressionNode();
            subExpression.opCode = Token.Multiply;
            subExpression.left = second2;
            subExpression.right = one;
            expectedTree.opCode = Token.Plus;
            expectedTree.left = first2;
            expectedTree.right = subExpression;

            lexer.Code = "А = 2 + (2 * 1)";
            Assert.IsTrue(parser.Build(lexer));
            node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expectedTree));
            
        }

        [TestMethod]
        public void Unary_Minus()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            lexer.Code = "А = (-2);";
            var parser = new Parser(builder);
            Assert.IsTrue(parser.Build(lexer));
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(node.Right is UnaryExpressionNode);

            var unary = new UnaryExpressionNode();
            unary.opCode = Token.UnaryMinus;
            unary.operand = new OperandNode() { content = "2" };
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, unary));

            lexer.Code = "А = -2 + 1;";
            Assert.IsTrue(parser.Build(lexer));
            var expected = new BinExpressionNode();
            expected.opCode = Token.Plus;
            expected.left = unary;
            expected.right = new OperandNode() { content = "1" };
            
            node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expected));

            lexer.Code = "А = Не -2";
            Assert.IsTrue(parser.Build(lexer));
            var notNode = new UnaryExpressionNode() { opCode = Token.Not, operand = unary };
            node = builder.topNode as AssignmentNode;
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, notNode));

            lexer.Code = "А = -Не 2";
            try
            {
                parser.Build(lexer);
            }
            catch(CompilerException e)
            {
                Assert.IsTrue(e.Message.Contains("Ожидается выражение"));
            }
        }

        [TestMethod]
        public void FunctionCall_OneArgument()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);
            lexer.Code = "А = Б(-2);";
            Assert.IsTrue(parser.Build(lexer));

            var args = new IASTNode[1];
            args[0] = new UnaryExpressionNode()
            {
                opCode= Token.UnaryMinus, 
                operand = new OperandNode("2")
            };

            var expected = new CallNode("Б", args);
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expected));

        }

        [TestMethod]
        public void FunctionCall_Many_Arguments()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);
            lexer.Code = "А = Б(-2,1,3);";
            Assert.IsTrue(parser.Build(lexer));

            var args = new IASTNode[3];
            args[0] = new UnaryExpressionNode()
            {
                opCode = Token.UnaryMinus,
                operand = new OperandNode("2")
            };
            args[1] = new OperandNode("1");
            args[2] = new OperandNode("3");

            var expected = new CallNode("Б", args);
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expected));
        }

        [TestMethod]
        public void FunctionCall_NoArguments()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);

            // no args
            lexer.Code = "А = Б();";
            Assert.IsTrue(parser.Build(lexer));

            var args = new IASTNode[0];
            var expected = new CallNode("Б", args);
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expected));
        }

        [TestMethod]
        public void FunctionCall_Missing_Arguments()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);

            lexer.Code = "А = Б(-2,,,3,);";
            Assert.IsTrue(parser.Build(lexer));

            var args = new IASTNode[5];
            args[0] = new UnaryExpressionNode()
            {
                opCode = Token.UnaryMinus,
                operand = new OperandNode("2")
            };
            args[3] = new OperandNode("3");

            var expected = new CallNode("Б", args);
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expected));

        }

        [TestMethod]
        public void Unclosed_Arguments_List()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);

            Action checkAction = () =>
                {
                    bool thrown = false;
                    try
                    {
                        parser.Build(lexer);
                    }
                    catch (CompilerException)
                    {
                        thrown = true;
                    }

                    Assert.IsTrue(thrown);
                };

            lexer.Code = "А = Б(-2,,,3,";
            checkAction();

            lexer.Code = "А = Б(1";
            checkAction();

            lexer.Code = "А = Б(1;";
            checkAction();

            lexer.Code = "А = Б(";
            checkAction();
        }

        [TestMethod]
        public void Property_Access_Read()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);

            lexer.Code = "А = Б.В;";
            Assert.IsTrue(parser.Build(lexer));

            lexer.Code = "А = Б.В.Г;";
            Assert.IsTrue(parser.Build(lexer));

            lexer.Code = "А = Б.В().Г;";
            Assert.IsTrue(parser.Build(lexer));

            lexer.Code = "А = Б.В[0][1].X();";
            Assert.IsTrue(parser.Build(lexer));

            lexer.Code = "А = Б.В()[Г];";
            Assert.IsTrue(parser.Build(lexer));
        }

        [TestMethod]
        public void CodeBatch()
        {
            var code = @"Перем А;
                       А[U] = 1;
                       Б.X = 2";

            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);

            lexer.Code = code;
            Assert.IsTrue(parser.Build(lexer));
            Assert.IsTrue(builder.Variables[0] == "А");
            Assert.IsTrue(builder.CodeNode.Children.Count == 2);

            TestASTNodeBase left = new IndexedAccessNode(
                new OperandNode("А"),
                new OperandNode("U"));

            TestASTNodeBase expected = new AssignmentNode(left, new OperandNode("1"));
            Assert.IsTrue(TestASTNodeBase.CompareTrees(builder.CodeNode.Children[0], expected));

            left = new PropertyAccessNode(new OperandNode("Б"), "X");
            expected = new AssignmentNode(left, new OperandNode("2"));
            Assert.IsTrue(TestASTNodeBase.CompareTrees(builder.CodeNode.Children[1], expected));

        }

        [TestMethod]
        public void ModuleVariables_NoNeed_For_Semicolon_In_Last_Statement()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);

            lexer.Code = @"Перем А,Б Экспорт;
                           Перем Б";

            Assert.IsTrue(parser.Build(lexer));
            Assert.IsTrue(builder.Variables.Count == 3);
            Assert.IsTrue(builder.Variables[0] == "А");
            Assert.IsTrue(builder.Variables[1] == "Б export");
            Assert.IsTrue(builder.Variables[2] == "Б");
        }

        [TestMethod]
        public void LateVarDefinition_On_TopLevel_Semicolon()
        {
            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);

            lexer.Code = @"Перем А,Б Экспорт;
                           ;
                           Перем Б";

            try
            {
                parser.Build(lexer);
                Assert.Fail("Exception is not thrown");
            }
            catch (CompilerException e)
            {
                Assert.IsTrue(e.Message.Contains("Объявления переменных должны быть расположены"));
            }

        }
    }

    abstract class TestASTNodeBase : IASTNode, IEquatable<IASTNode>
    {
        public bool Equals(IASTNode other)
        {
            if (other == null)
                return false;

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
        public OperandNode()
        {

        }

        public OperandNode(string content)
        {
            this.content = content;
        }

        public string content;
        protected override bool EqualsInternal(IASTNode other)
        {
            return (other as OperandNode).content == content;
        }
    }

    class CallNode : TestASTNodeBase
    {

        string name;
        IASTNode[] arguments;

        public CallNode(string name, IASTNode[] args)
        {
            this.name = name;
            this.arguments = args;
        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var callNode = other as CallNode;
            bool argIsOk = callNode.arguments.Length == this.arguments.Length;
            if(argIsOk)
                for (int i = 0; i < callNode.arguments.Length; i++)
                {
                    if(this.arguments[i] == null)
                    {
                        argIsOk = argIsOk && callNode.arguments[i] == null;
                    }
                    else
                    {
                        argIsOk = argIsOk && ((TestASTNodeBase)(this.arguments[i])).Equals(callNode.arguments[i]);
                    }
                    if (!argIsOk)
                        break;
                }

            return argIsOk && this.name == callNode.name;
        }
    }

    class AssignmentNode : TestASTNodeBase
    {
        public IASTNode Left;
        public IASTNode Right;

        public AssignmentNode(IASTNode left, IASTNode right)
        {
            Left = left;
            Right = right;
        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var left = (TestASTNodeBase)Left;
            var right = (TestASTNodeBase)Right;

            var otherTest = (AssignmentNode)other;

            return left.Equals(otherTest.Left) && right.Equals(otherTest.Right);
        }
    }

    class CodeBatchNode : TestASTNodeBase
    {
        List<TestASTNodeBase> _children = new List<TestASTNodeBase>();

        public List<TestASTNodeBase> Children
        {
            get { return _children; }
        }

        public void Add(IASTNode node)
        {
            var casted = (TestASTNodeBase)node;
            _children.Add(casted);

        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var casted = (CodeBatchNode)other;

            if(casted._children.Count != _children.Count)
                return false;

            bool allMatch = true;

            for (int i = 0; i < _children.Count; i++)
            {
                allMatch = allMatch && _children[i].Equals(casted._children[i]);
                if (!allMatch)
                    break;
            }

            return allMatch;

        }
    }

    class IndexedAccessNode : TestASTNodeBase
    {
        IASTNode _target;
        IASTNode _index;

        public IndexedAccessNode(IASTNode target, IASTNode index)
        {
            _target = target;
            _index = index;
        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var otherTest = (IndexedAccessNode)other;
            var target = (TestASTNodeBase)_target;
            var index = (TestASTNodeBase)_index;

            return target.Equals(otherTest._target) && index.Equals(otherTest._index);

        }
    }

    class PropertyAccessNode : TestASTNodeBase
    {
        IASTNode _target;
        string _name;

        public PropertyAccessNode(IASTNode target, string name)
        {
            _target = target;
            _name = name;
        }

        protected override bool EqualsInternal(IASTNode other)
        {
            var otherTest = (PropertyAccessNode)other;
            var target = (TestASTNodeBase)_target;
            
            return target.Equals(otherTest._target) && _name.Equals(otherTest._name);

        }
    }

    class Builder : IModuleBuilder
    {

        List<string> _variables = new List<string>();
        CodeBatchNode _mainCode = new CodeBatchNode();

        public IASTNode topNode
        {
            get
            {
                return _mainCode.Children[0];
            }
        }

        public List<string> Variables
        {
            get
            {
                return _variables;
            }
        }

        public CodeBatchNode CodeNode
        {
            get
            {
                return _mainCode;
            }
        }

        public void BeginModule()
        {
            _variables.Clear();
            _mainCode.Children.Clear();
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
            _variables.Add(symbolicName + " export");
        }

        public void DefineVariable(string symbolicName)
        {
            _variables.Add(symbolicName);
        }

        public IASTNode SelectOrUseVariable(string identifier)
        {
            return ReadVariable(identifier);
        }

        public void BuildAssignment(IASTNode acceptor, IASTNode source)
        {
            _mainCode.Add(new AssignmentNode(acceptor, source));
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
            if (token == Token.Minus || token == Token.UnaryMinus)
                token = Token.UnaryMinus;
            else if (token != Token.Not)
                throw new ArgumentException();

            return new UnaryExpressionNode() { opCode = token, operand = operandNode };

        }

        public IASTNode BuildFunctionCall(IASTNode target, string identifier, IASTNode[] args)
        {
            return new CallNode(identifier, args);
        }


        public IASTNode ResolveProperty(IASTNode target, string p)
        {
            return new PropertyAccessNode(target, p);
        }

        public IASTNode BuildIndexedAccess(IASTNode target, IASTNode expr)
        {
            return new IndexedAccessNode(target, expr);
        }


        public void BuildProcedureCall(IASTNode resolved, string ident, IASTNode[] args)
        {
            throw new NotImplementedException();
        }
    }
}
