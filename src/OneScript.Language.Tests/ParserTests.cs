/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using Xunit;
using FluentAssertions;
using OneScript.Language.SyntaxAnalysis.AstNodes;

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
            
            var treeValidator = ParseModuleAndGetValidator(code);

            treeValidator.Is(NodeKind.VariablesSection);

            var child = treeValidator.NextChild();
            child.Is(NodeKind.VariableDefinition)
                .WithNode(NodeKind.Identifier)
                .Equal("П1");

            child = treeValidator.NextChild();
            child.Is(NodeKind.VariableDefinition)
                .WithNode(NodeKind.Identifier)
                .Equal("П2");
            child.HasNode(NodeKind.ExportFlag);
            
            child = treeValidator.NextChild();
            child.Is(NodeKind.VariableDefinition)
                .WithNode(NodeKind.Annotation)
                .Equal("Аннотация");
            
            child.HasNode(NodeKind.Identifier)
                .Equal("П3");
            
            child = treeValidator.NextChild();
            child.Is(NodeKind.VariableDefinition).WithNode(NodeKind.Identifier).Equal("П4");
            child.HasNode(NodeKind.ExportFlag);
            
            child = treeValidator.NextChild();
            child.Is(NodeKind.VariableDefinition).WithNode(NodeKind.Identifier).Equal("П5");
            child.HasNode(NodeKind.ExportFlag);
        }

        [Fact]
        public void CheckBuild_Of_Methods_Section()
        {
            var code = "Процедура А() КонецПроцедуры Функция Б() КонецФункции";
            var node = ParseModuleAndGetValidator(code);

            node.Is(NodeKind.MethodsSection);
            node.CurrentNode.ChildrenList.Should().HaveCount(2, "two methods in code");

            var methodNode = node.NextChild();
            methodNode.Is(NodeKind.Method)
                .NextChildIs(NodeKind.MethodSignature)
                .DownOneLevel()
                    .NextChildIs(NodeKind.Procedure)
                    .NextChildIs(NodeKind.Identifier).ChildItself()
                    .Equal("А");
            
            methodNode = node.NextChild();
            methodNode.Is(NodeKind.Method)
                .NextChildIs(NodeKind.MethodSignature)
                .DownOneLevel()
                .NextChildIs(NodeKind.Function)
                    .HasNode(NodeKind.Identifier)
                    .Equal("Б");
        }

        [Fact]
        public void Check_Annotation_Parameters()
        {
            var code = @"
            &БезПараметров
            &СИменемПараметра(Имя)
            &НесколькоПараметров(Имя, Имя2)
            &Литерал(""Привет"")
            &ИмяИЗначение(А = ""Привет"", М = 1)
            Перем УзелВладелец;";

            var variable = ParseModuleAndGetValidator(code).NextChild();

            var anno = variable.NextChild();
            anno.Is(NodeKind.Annotation)
                .NoMoreChildren();
            anno.Equal("БезПараметров");

            anno = variable.NextChild()
                .Is(NodeKind.Annotation);
            anno.Equal("СИменемПараметра");
            anno.DownOneLevel().Is(NodeKind.AnnotationParameter)
                    .NextChildIs(NodeKind.AnnotationParameterName)
                    .NoMoreChildren();
            anno.NoMoreChildren();

            anno = variable.NextChild().Is(NodeKind.Annotation);
            anno.Equal("НесколькоПараметров");
            anno.HasChildNodes(2);
            anno.NextChild().Is(NodeKind.AnnotationParameter)
                .NextChildIs(NodeKind.AnnotationParameterName)
                .NoMoreChildren();
            
            anno.NextChild().Is(NodeKind.AnnotationParameter)
                .NextChildIs(NodeKind.AnnotationParameterName)
                .NoMoreChildren();

            anno = variable.NextChild();
            anno.Equal("Литерал");
            var param = anno.NextChild().Is(NodeKind.AnnotationParameter);
            anno.NoMoreChildren();
            param.NextChildIs(NodeKind.AnnotationParameterValue).Equals("Привет");
            param.NoMoreChildren();
            
            anno = variable.NextChild();
            anno.Equal("ИмяИЗначение");
            anno.NextChild().Is(NodeKind.AnnotationParameter)
                .NextChildIs(NodeKind.AnnotationParameterName)
                .NextChildIs(NodeKind.AnnotationParameterValue)
                .NoMoreChildren();
            anno.NextChild().Is(NodeKind.AnnotationParameter)
                .NextChildIs(NodeKind.AnnotationParameterName)
                .NextChildIs(NodeKind.AnnotationParameterValue)
                .NoMoreChildren();
                
        }

        [Fact]
        public void Check_Method_Parameters()
        {
            var code = @"
            Процедура П(А, Знач А, Б = 1, Знач Д = -10) Экспорт КонецПроцедуры";

            var proc = ParseModuleAndGetValidator(code).NextChild();

            var signature = proc.NextChild().Is(NodeKind.MethodSignature);
            signature
                .NextChildIs(NodeKind.Procedure)
                .NextChildIs(NodeKind.Identifier)
                .NextChildIs(NodeKind.MethodParameters)
                .NextChildIs(NodeKind.ExportFlag)
                .NoMoreChildren();

            var paramList = signature.HasNode(NodeKind.MethodParameters);
            paramList.NextChild().Is(NodeKind.MethodParameter)
                .NextChildIs(NodeKind.Identifier).ChildItself()
                .Equal("А");
            
            paramList.NextChild().Is(NodeKind.MethodParameter)
                .NextChildIs(NodeKind.ByValModifier)
                .NextChildIs(NodeKind.Identifier)
                .NoMoreChildren();
            
            paramList.NextChild().Is(NodeKind.MethodParameter)
                .NextChildIs(NodeKind.Identifier)
                .NextChildIs(NodeKind.ParameterDefaultValue)
                .ChildItself().Equal("1");
            
            paramList.NextChild().Is(NodeKind.MethodParameter)
                .NextChildIs(NodeKind.ByValModifier)
                .NextChildIs(NodeKind.Identifier)
                .NextChildIs(NodeKind.ParameterDefaultValue)
                .ChildItself().Equal("-10");
        }
        
        [Fact]
        public void Check_Method_Variables()
        {
            var code = @"
            Процедура П()
                Перем А;
                Перем Б;
                Код = Истина;
            КонецПроцедуры";

            var proc = ParseModuleAndGetValidator(code)
                .NextChild()
                .CastTo<MethodNode>();

            proc.VariableDefinitions().Should().HaveCount(2);

        }

        [Fact]
        public void Check_Statement_GlobalFunctionCall()
        {
            var batch = ParseBatchAndGetValidator("Proc();");
            batch.Is(NodeKind.CodeBatch);
            var node = batch.NextChild();
            node.Is(NodeKind.GlobalCall)
                .HasNode(NodeKind.Identifier)
                .Equal("Proc");

        }
        
        [Fact]
        public void Check_Statement_ObjectMethod_Call()
        {
            var code = @"Target.Call();
            Target().Call();
            Target[0].Call();
            Target.SubCall().Call()";
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.DereferenceOperation)
                    .NextChildIs(NodeKind.Identifier)
                    .NextChildIs(NodeKind.MethodCall);

            node = batch.NextChild();
            node.Is(NodeKind.DereferenceOperation)
                .NextChildIs(NodeKind.GlobalCall)
                .NextChildIs(NodeKind.MethodCall);
            
            node = batch.NextChild();
            node.Is(NodeKind.DereferenceOperation)
                .NextChildIs(NodeKind.IndexAccess)
                .NextChildIs(NodeKind.MethodCall);

            node = batch.NextChild();
            node.Is(NodeKind.DereferenceOperation)
                .NextChildIs(NodeKind.DereferenceOperation)
                .NextChildIs(NodeKind.MethodCall);
        }
        
        [Fact]
        public void Check_Argument_Passing()
        {
            var code = @"Proc();
            Proc(А+1, Б+2);
            Proc('00010101');
            Proc(,);
            Proc(1,);
            Proc(,1)";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            var node = batch.NextChild();
            node.Is(NodeKind.GlobalCall)
                .NextChild().Is(NodeKind.Identifier)
                .Equal("Proc");
            node.NextChild().Is(NodeKind.CallArgumentList)
                .NoMoreChildren();

            node = batch.NextChild();
            node.NextChild();
            var list = node.NextChild().Is(NodeKind.CallArgumentList);
            list.NextChildIs(NodeKind.CallArgument)
                .NextChildIs(NodeKind.CallArgument)
                .NoMoreChildren();

            list.CurrentNode.ChildrenList[0].ChildrenList[0].Kind.Should().Be(NodeKind.BinaryOperation);
            list.CurrentNode.ChildrenList[1].ChildrenList[0].Kind.Should().Be(NodeKind.BinaryOperation);

            node = batch.NextChild();
            node.NextChild();
            list = node.NextChild();
            list.HasChildNodes(1);

            node = batch.NextChild();
            node.NextChild();
            list = node.NextChild();
            list.HasChildNodes(2);
            list.NextChild().Is(NodeKind.CallArgument).NoMoreChildren();
            list.NextChild().Is(NodeKind.CallArgument).NoMoreChildren();
            
            node = batch.NextChild();
            node.NextChild();
            list = node.NextChild();
            list.HasChildNodes(2);
            list.NextChild().Is(NodeKind.CallArgument).NextChildIs(NodeKind.Constant);
            list.NextChild().Is(NodeKind.CallArgument).NoMoreChildren();
            
            node = batch.NextChild();
            node.NextChild();
            list = node.NextChild();
            list.HasChildNodes(2);
            list.NextChild().Is(NodeKind.CallArgument).NoMoreChildren();
            list.NextChild().Is(NodeKind.CallArgument).NextChildIs(NodeKind.Constant);
            
        }
        
        [Fact]
        public void Check_Assignment_OnVariable()
        {
            var code = @"Target = 1";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.Assignment)
                .NextChildIs(NodeKind.Identifier)
                .NextChildIs(NodeKind.Constant);
        }
        
        [Fact]
        public void Check_Assignment_OnProperty()
        {
            var code = @"Target.Prop = 1";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.Assignment)
                .NextChildIs(NodeKind.DereferenceOperation)
                .NextChildIs(NodeKind.Constant);
        }
        
        [Fact]
        public void Check_Assignment_OnIndex()
        {
            var code = @"Target[0] = 1";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.Assignment)
                .NextChildIs(NodeKind.IndexAccess)
                .NextChildIs(NodeKind.Constant);
        }
        
        [Fact]
        public void Check_Assignment_OnComplex_Chain()
        {
            var code = @"Target[0].SomeProp.Method(Object.Prop[3*(8-2)].Data).Prop = ?(Data = True, Object[0], Object.Method()[12]);";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.Assignment)
                .NextChildIs(NodeKind.DereferenceOperation)
                .NextChildIs(NodeKind.TernaryOperator);
        }
        
        [Fact]
        public void Check_Binary_And_Unary_Expressions()
        {
            var code = @"А = -2 + 2";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);

            var assignment = batch
                .NextChild().Is(NodeKind.Assignment);
            assignment.NextChild();
            var node = assignment.NextChild();
            node.Is(NodeKind.BinaryOperation)
                .Equal(Token.Plus.ToString());
            node.NextChild().Is(NodeKind.UnaryOperation)
                .NextChildIs(NodeKind.Constant);
            node.NextChild().Is(NodeKind.Constant);
        }
        
        [Fact]
        public void Check_Binary_Expressions_Chain()
        {
            var code = @"А = 2 + 2 + 3 - 1";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);

            var assignment = batch
                .NextChild().Is(NodeKind.Assignment);
            assignment.NextChild();
            var node = assignment.NextChild();
            node.Is(NodeKind.BinaryOperation)
                .Equal(Token.Minus.ToString());
            node.NextChildIs(NodeKind.BinaryOperation)
                .NextChildIs(NodeKind.Constant);
        }

        [Fact]
        public void AdditionPriority_For_Strings()
        {
            var code = @"А = ""Стр1"" + Сч + ""Стр2""";
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);

            var assignment = batch
                .NextChild().Is(NodeKind.Assignment);
            assignment.NextChild();
            var node = assignment.NextChild();
            node.Is(NodeKind.BinaryOperation).Equal(Token.Plus.ToString());

            var firstAddition = node.NextChild();
            firstAddition.Is(NodeKind.BinaryOperation)
                .NextChildIs(NodeKind.Constant)
                .NextChildIs(NodeKind.Identifier);
            
            node.NextChild().Is(NodeKind.Constant)
                .Equal("Стр2");
        }
        
        
        [Fact]
        public void Check_Binary_Expressions_Priority()
        {
            var code = @"А = 2 + 2 * 3";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);

            var assignment = batch
                .NextChild().Is(NodeKind.Assignment);
            assignment.NextChild();
            var node = assignment.NextChild();
            node.Is(NodeKind.BinaryOperation)
                .Equal(Token.Plus.ToString());
            node.NextChild();
            node.NextChild().Is(NodeKind.BinaryOperation)
                .Equal(Token.Multiply.ToString());
        }
        
        [Fact]
        public void Check_Binary_Division()
        {
            var code = @"А = Variable/Variable;";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);

            var assignment = batch
                .NextChild().Is(NodeKind.Assignment);
            assignment.NextChild();
            var node = assignment.NextChild();
            node.Is(NodeKind.BinaryOperation)
                .Equal(Token.Division.ToString());
        }
        
        [Fact] 
        public void Check_Binary_Modulo()
        {
            var code = @"А = Variable%Variable;";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);

            var assignment = batch
                .NextChild().Is(NodeKind.Assignment);
            assignment.NextChild();
            var node = assignment.NextChild();
            node.Is(NodeKind.BinaryOperation)
                .Equal(Token.Modulo.ToString());
        }
        
        [Fact]
        public void Check_Logical_Expressions()
        {
            var code = @"Переменная >= 2 ИЛИ Не Переменная < 1";
            
            var expr = ParseExpressionAndGetValidator(code);
            expr.Is(NodeKind.BinaryOperation)
                .Equal(Token.Or.ToString());

            expr.NextChild().Is(NodeKind.BinaryOperation)
                .Equal(Token.MoreOrEqual.ToString());
            
            expr.NextChild().Is(NodeKind.UnaryOperation)
                .NextChild().Is(NodeKind.BinaryOperation)
                .Equal(Token.LessThan.ToString());
        }

        [Fact]
        public void Check_EqualExpression_Is_Comparison_But_Not_Assignment()
        {
            var code = @"Переменная = 2";
            
            var expr = ParseExpressionAndGetValidator(code);
            expr.Is(NodeKind.BinaryOperation)
                .Equal(Token.Equal.ToString());
        }
        
        [Fact]
        public void Check_Logical_Priority_Direct()
        {
            var code = @"Переменная1 ИЛИ Переменная2 И Переменная3";

            var expr = ParseExpressionAndGetValidator(code);
            expr.Is(NodeKind.BinaryOperation)
                .Equal(Token.Or.ToString());
        }
        
        [Fact]
        public void Check_Logical_Priority_Parenthesis()
        {
            var code = @"(Переменная1 ИЛИ Переменная2) И Переменная3";
            
            var expr = ParseExpressionAndGetValidator(code);
            expr.Is(NodeKind.BinaryOperation)
                .Equal(Token.And.ToString());
        }

        [Fact]
        public void Check_Keywords_As_PropertyNames()
        {
            var code = @"ТипЗначенияJson.Null";
            
            var expr = ParseExpressionAndGetValidator(code);
            expr.Is(NodeKind.DereferenceOperation)
                .NextChildIs(NodeKind.Identifier)
                .NextChildIs(NodeKind.Identifier);
        }
        
        [Fact]
        public void Can_Dereference_After_Construction()
        {
            var code = @"ТекПуть = Новый Файл(ТекущийСценарий().Источник).Путь;";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.Assignment);
            node.NextChild();
            var rightSide = node.NextChild();
            rightSide.Is(NodeKind.DereferenceOperation)
                .NextChildIs(NodeKind.NewObject)
                .NextChildIs(NodeKind.Identifier);

        }
        
        [Fact]
        public void Explicit_Parameter_Skipping()
        {
            var code = @"Метод(,)";
            
            var call = ParseExpressionAndGetValidator(code);
            call.Is(NodeKind.GlobalCall);
            call.NextChild();
            call.NextChild()
                .CurrentNode.Children.Should().HaveCount(2);
        }

        [Fact]
        public void Throws_When_Last_ParameterDefinition_IsMissing()
        {
            var code = @"Процедура ПодключитьсяКХранилищу(Знач СтрокаСоединения, 
                                            Знач ПользовательХранилища,
                                            Знач ПарольХранилища = """",
                                            Знач ИгнорироватьНаличиеПодключеннойБД = Ложь,
                                            Знач ЗаменитьКонфигурациюБД = Истина,
                                           	) Экспорт
            КонецПроцедуры";

            var parser = DefaultBslParser.PrepareParser(code);
            parser.ParseStatefulModule();

            parser.Errors.Should().NotBeEmpty("last parameter is missing");
            parser.Errors.First().ErrorId.Should().Be("IdentifierExpected");
        }
        
        [Fact]
        public void Check_If_With_No_Alternatives()
        {
            var code =
                @"Если А = 1 Тогда
                    Б = 0;
                КонецЕсли;";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.Condition);
            node.NextChildIs(NodeKind.BinaryOperation)
                .NextChildIs(NodeKind.CodeBatch)
                .NextChildIs(NodeKind.BlockEnd)
                .NoMoreChildren();
        }
        
        [Fact]
        public void Check_If_With_Else_Alternative()
        {
            var code =
                @"Если А = 1 Тогда
                    ;
                Иначе
                    ;
                КонецЕсли;";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.Condition);
            node.NextChildIs(NodeKind.BinaryOperation)
                .NextChildIs(NodeKind.CodeBatch)
                .NextChildIs(NodeKind.CodeBatch)
                .NextChildIs(NodeKind.BlockEnd)
                .NoMoreChildren();
        }
        
        [Fact]
        public void Check_If_With_ElseIf_Alternatives()
        {
            var code =
                @"Если А = 1 Тогда
                    ;
                ИначеЕсли Б = 2 Тогда
                    ;
                ИначеЕсли Б = 2 Тогда
                    ;
                КонецЕсли;";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.Condition);
            node.NextChildIs(NodeKind.BinaryOperation)
                .NextChildIs(NodeKind.CodeBatch)
                .NextChildIs(NodeKind.Condition)
                .NextChildIs(NodeKind.Condition)
                .NextChildIs(NodeKind.BlockEnd)
                .NoMoreChildren();
        }
        
        [Fact]
        public void Check_If_With_ElseIf_And_Else_Alternatives()
        {
            var code =
                @"Если А = 1 Тогда
                    ;
                ИначеЕсли Б = 2 Тогда
                    ;
                ИначеЕсли Б = 2 Тогда
                    ;
                Иначе
                    ;
                КонецЕсли;";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.Condition);
            node.NextChildIs(NodeKind.BinaryOperation)
                .NextChildIs(NodeKind.CodeBatch)
                .NextChildIs(NodeKind.Condition)
                .NextChildIs(NodeKind.Condition)
                .NextChildIs(NodeKind.CodeBatch)
                .NextChildIs(NodeKind.BlockEnd)
                .NoMoreChildren();

            var altNodes = ((ConditionNode) node.CurrentNode.RealNode).GetAlternatives();
            altNodes.Should().HaveCount(3);
        }
        
        [Fact]
        public void Check_While_Statement()
        {
            var code =
                @"Пока А = 1 Цикл
                    ;
                КонецЦикла";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.WhileLoop);
            node.NextChildIs(NodeKind.BinaryOperation)
                .NextChildIs(NodeKind.CodeBatch)
                .NextChildIs(NodeKind.BlockEnd)
                .NoMoreChildren();
        }
        
        [Fact]
        public void Check_Foreach_Statement()
        {
            var code =
                @"Для Каждого Итератор Из Коллекция Цикл
                    ;
                КонецЦикла";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.ForEachLoop);
            node.NextChildIs(NodeKind.ForEachVariable)
                .NextChildIs(NodeKind.ForEachCollection)
                .NextChildIs(NodeKind.CodeBatch)
                .NextChildIs(NodeKind.BlockEnd)
                .NoMoreChildren();
        }

        [Fact]
        public void CheckHandler_Expression_And_EventName()
        {
            var code = "ДобавитьОбработчик ЧтоТо[Индекс].ИмяСобытия, ЧтоТо().ИмяОбработчика";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.AddHandler);
            node.NextChildIs(NodeKind.DereferenceOperation)
                .NextChildIs(NodeKind.DereferenceOperation)
                .NoMoreChildren();
        }
        
        [Fact]
        public void CheckHandler_Expression_And_ProcedureName()
        {
            var code = "ДобавитьОбработчик ЧтоТо[Индекс].ИмяСобытия, ИмяОбработчика";
            
            var batch = ParseBatchAndGetValidator(code);
            batch.Is(NodeKind.CodeBatch);
            
            var node = batch.NextChild();
            node.Is(NodeKind.AddHandler);
            node.NextChildIs(NodeKind.DereferenceOperation)
                .NextChildIs(NodeKind.Identifier)
                .NoMoreChildren();
        }
        
        [Fact]
        public void Check_AstDirectives_Creation()
        {
            var code = @"#Использовать АБВ
                        #Использовать ""аааа""
                        #Область Продукты
                        #КонецОбласти
                        #ВерсияЯзыка 2.0.8
                        #ЧтоТоЕще АбраКадабра(>= 18, ??)";
            
            var module = ParseModuleAndGetValidator(code);
            var batch = new SyntaxTreeValidator(new TestAstNode(module.CurrentNode.RealNode.Parent));
            batch.Is(NodeKind.Module);
            
            var node = batch.NextChild();
            node.Is(NodeKind.Preprocessor)
                .Equal("Использовать");
            node.NextChild().Is(NodeKind.Unknown)
                .Equal("АБВ");
            
            node = batch.NextChild();
            node.Is(NodeKind.Preprocessor)
                .Equal("Использовать");
            node.NextChild().Is(NodeKind.Unknown)
                .Equal("\"аааа\"");
            
            node = batch.NextChild();
            node.Is(NodeKind.Preprocessor)
                .Equal("Область");
            node.NextChild().Is(NodeKind.Unknown)
                .Equal("Продукты");
            
            node = batch.NextChild();
            node.Is(NodeKind.Preprocessor)
                .Equal("КонецОбласти");
            node.NoMoreChildren();
            
            node = batch.NextChild();
            node.Is(NodeKind.Preprocessor)
                .Equal("ВерсияЯзыка");
            node.NextChild().Is(NodeKind.Unknown)
                .Equal("2.0.8");
            
            node = batch.NextChild();
            node.Is(NodeKind.Preprocessor)
                .Equal("ЧтоТоЕще");
            node.NextChild().Is(NodeKind.Unknown)
                .Equal("АбраКадабра(>= 18, ??)");

            batch.NoMoreChildren();
        }
        
        private static SyntaxTreeValidator ParseModuleAndGetValidator(string code)
        {
            return MakeValidator(code, p => p.ParseStatefulModule());
        }
        
        private static SyntaxTreeValidator ParseBatchAndGetValidator(string code)
        {
            var body = MakeValidator(code, p => p.ParseCodeBatch());
            return body.NextChild();
        }
        
        private static SyntaxTreeValidator ParseExpressionAndGetValidator(string code)
        {
            return MakeValidator(code, p => p.ParseExpression());
        }

        private static SyntaxTreeValidator MakeValidator(string code, Func<DefaultBslParser, BslSyntaxNode> action)
        {
            var lexer = new DefaultLexer();
            lexer.Code = code;

            var client = new DefaultAstBuilder();
            var parser = new DefaultBslParser(client, lexer);
            parser.DirectiveHandlers.Add(new AstNodeAppendingHandler(client));
            var node = action(parser) as BslSyntaxNode;

            node.Should().NotBeNull();
            parser.Errors.Should().BeEmpty("the valid code is passed");
            var treeValidator = new SyntaxTreeValidator(new TestAstNode(node.Children.First()));
            return treeValidator;
        }

    }
}