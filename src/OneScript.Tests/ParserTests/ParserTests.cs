using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneScript.Scripting;
using System.Collections.Generic;
using OneScript.Scripting.Compiler;
using OneScript.Scripting.Compiler.Lexics;

namespace OneScript.Tests
{
    [TestClass]
    public class ParserTests
    {
        private Builder ParseCode(string code)
        {
            var compiler = CompilerFactory<Builder>.Create();
            compiler.SetCode(code);
            var builder = (Builder)compiler.GetModuleBuilder();

            Assert.IsTrue(compiler.Compile());

            return (Builder)compiler.GetModuleBuilder();
        }

        [TestMethod]
        public void TopLevel_Semicolons()
        {
            var builder = ParseCode(";; А = 1;;");
            Assert.IsTrue(builder.topNode is AssignmentNode);
            Assert.IsTrue((builder.topNode as AssignmentNode).Right is OperandNode);
        }

        [TestMethod]
        public void Empty_Code_Batch()
        {
            var builder = ParseCode("Если Истина Тогда КонецЕсли");
            Assert.IsTrue(builder.topNode is ConditionNode);

            var ifNode = builder.topNode as ConditionNode;
            var trueBatch = ifNode.TruePart as CodeBatchNode;

            Assert.IsNotNull(trueBatch);
            Assert.AreEqual(0, trueBatch.Children.Count);

        }

        [TestMethod]
        public void Empty_Module()
        {
            var builder = ParseCode("");
            Assert.IsNull(builder.topNode);
            Assert.AreEqual(0, builder.Variables.Count);
            Assert.AreEqual(0, builder.Methods.Count);
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

            var builder = ParseCode("А = 2 + 2 + 1");
            
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
            
            var builder = ParseCode("А = 2 + 2 * 1");
            
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expectedTree));
        }

        [TestMethod]
        public void Expression_Parenthesis_Priority_AdditionFirst()
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

            var builder = ParseCode("А = (2 + 2) * 1");
            
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expectedTree));

        }

        [TestMethod]
        public void Expression_Parenthesis_Priority_Straight()
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

            var builder = ParseCode("А = 2 + (2 * 1)");
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase) node.Right, expectedTree));
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
            var builder = ParseCode("А = Б(-2);");
            
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
            var builder = ParseCode("А = Б(-2,1,3);");
            
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
            var builder = ParseCode("А = Б();");
            
            var args = new IASTNode[0];
            var expected = new CallNode("Б", args);
            var node = builder.topNode as AssignmentNode;
            Assert.IsNotNull(node);
            Assert.IsTrue(TestASTNodeBase.CompareTrees((TestASTNodeBase)node.Right, expected));
        }

        [TestMethod]
        public void FunctionCall_Missing_Arguments()
        {
            var builder = ParseCode("А = Б(-2,,,3,);");
            
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

            var builder = ParseCode(code);
            
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
            var code = @"Перем А,Б Экспорт;
                           Перем Б";

            var builder = ParseCode(code);
            
            Assert.IsTrue(builder.Variables.Count == 3);
            Assert.IsTrue(builder.Variables[0] == "А");
            Assert.IsTrue(builder.Variables[1] == "Б export");
            Assert.IsTrue(builder.Variables[2] == "Б");
        }

        [TestMethod]
        public void LateVarDefinition_On_TopLevel_Semicolon()
        {
            try
            {
                ParseCode(@"Перем А,Б Экспорт;
                           ;
                           Перем Б");

                Assert.Fail("Exception is not thrown");
            }
            catch (CompilerException e)
            {
                Assert.IsTrue(e.Message.Contains("Объявления переменных должны быть расположены"));
            }

        }

        [TestMethod]
        public void MethodBuild()
        {
            var expected = new MethodNode("Название", false);
            var parameters = new[]
            {
                new ASTMethodParameter() {Name = "А"},
                new ASTMethodParameter() {ByValue = true, Name = "Б"},
                new ASTMethodParameter()
                {
                    ByValue = true,
                    DefaultValueLiteral = new ConstDefinition()
                    {
                        Presentation = "123",
                        Type = ConstType.Number
                    },
                    IsOptional = true
                }
            };

            expected.SetSignature(parameters, false);

            var builder = ParseCode(@"Процедура Название(А, Знач Б, Знач В = 123)
                            А = 1;
                            М = А + 1;
                        КонецПроцедуры");

            var node = builder.Methods[0];
            Assert.IsInstanceOfType(node, typeof(MethodNode));
            Assert.IsTrue(expected.Equals(node));
        }

        [TestMethod]
        public void ConditionBuild()
        {
            var code = @"Если А=Б Тогда
                             А = 1;
                         ИначеЕсли Б Тогда
                             X=1;
                         ИначеЕсли Б Тогда
                             X=1;
                         Иначе
                            Б = 2;
                            Г = 4;  
                         КонецЕсли";

            var builder = new Builder();
            var lexer = new Lexer();
            var parser = new Parser(builder);

            lexer.Code = code;
            Assert.IsTrue(parser.Build(lexer));

            Assert.IsInstanceOfType(builder.topNode, typeof(ConditionNode));

            var cond = builder.topNode as ConditionNode;

            Assert.AreEqual(1, ((CodeBatchNode)cond.TruePart).Children.Count);
            Assert.IsInstanceOfType(cond.FalsePart, typeof(ConditionNode));

            var elif = cond.FalsePart as ConditionNode;
            Assert.IsInstanceOfType(elif.FalsePart, typeof(ConditionNode));
            elif = elif.FalsePart as ConditionNode;
            Assert.IsInstanceOfType(elif.FalsePart, typeof(CodeBatchNode));
            var elseNode = elif.FalsePart as CodeBatchNode;
            Assert.AreEqual(2, elseNode.Children.Count);


        }

        [TestMethod]
        public void LogicalExpression_With_Single_Variable()
        {
            var builder = ParseCode("Если А Тогда ; КонецЕсли");
            
            var ifNode = builder.topNode as ConditionNode;
            Assert.IsNotNull(ifNode);

            var operand = ifNode.Condition as OperandNode;
            Assert.IsNotNull(ifNode);

            Assert.AreEqual("А", operand.content);
        }
    }

}
