/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.Extensions;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Sources;
using OneScript.Values;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    public partial class StackMachineCodeGenerator : BslSyntaxWalker, ICompilerBackend
    {
        private readonly IErrorSink _errorSink;
        private readonly StackRuntimeModule _module;
        private SourceCode _sourceCode;
        private ICompilerContext _ctx;
        private List<ConstDefinition> _constMap = new List<ConstDefinition>();
        
        private readonly List<ForwardedMethodDecl> _forwardedMethods = new List<ForwardedMethodDecl>();
        private readonly Stack<NestedLoopInfo> _nestedLoops = new Stack<NestedLoopInfo>();

        public StackMachineCodeGenerator(IErrorSink errorSink)
        {
            _errorSink = errorSink;
            _module = new StackRuntimeModule(typeof(IRuntimeContextInstance));
        }
        
        public CodeGenerationFlags ProduceExtraCode { get; set; }

        public IDependencyResolver DependencyResolver { get; set; }
        
        public StackRuntimeModule CreateModule(ModuleNode moduleNode, SourceCode source, ICompilerContext context)
        {
            if (moduleNode.Kind != NodeKind.Module)
            {
                throw new ArgumentException($"Node must be a Module node");
            }

            _ctx = context;
            _sourceCode = source;

            return CreateImageInternal(moduleNode);
        }

        private StackRuntimeModule CreateImageInternal(ModuleNode moduleNode)
        {
            VisitModule(moduleNode);
            CheckForwardedDeclarations();
            
            _module.LoadAddress = _ctx.TopIndex();
            _module.Source = _sourceCode;
            
            return _module;
        }

        protected override void VisitModuleAnnotation(AnnotationNode node)
        {
            if (node.Kind == NodeKind.Import)
                HandleImportClause(node);

            var classAnnotation = new BslAnnotationAttribute(node.Name);
            classAnnotation.SetParameters(GetAnnotationParameters(node));
            _module.ModuleAttributes.Add(classAnnotation);
        }

        private void HandleImportClause(AnnotationNode node)
        {
            if(DependencyResolver == default)
                return;
            
            var libName = node.Children
                .Cast<AnnotationParameterNode>()
                .First()
                .Value
                .Content;
            
            try
            {
                DependencyResolver.Resolve(_sourceCode, libName);
                if(_ctx is ModuleCompilerContext moduleContext)
                    moduleContext.Update();
            }
            catch (CompilerException e)
            {
                var error = new CodeError
                {
                    Description = e.Message,
                    Position = e.GetPosition()?.LineNumber == default
                        ? MakeCodePosition(node.Location)
                        : e.GetPosition(),
                    ErrorId = nameof(CompilerException)
                };
                AddError(error);
            }
        }

        private void CheckForwardedDeclarations()
        {
            if (_forwardedMethods.Count > 0)
            {
                foreach (var item in _forwardedMethods)
                {
                    SymbolBinding methN;
                    try
                    {
                        methN = _ctx.GetMethod(item.identifier);
                    }
                    catch (SymbolNotFoundException)
                    {
                        AddError(LocalizedErrors.SymbolNotFound(item.identifier), item.location);
                        continue;
                    }

                    var scope = _ctx.GetScope(methN.ScopeNumber);

                    var methInfo = scope.Methods[methN.MemberNumber].Method;
                    Debug.Assert(StringComparer.OrdinalIgnoreCase.Compare(methInfo.Name, item.identifier) == 0);
                    if (item.asFunction && !methInfo.IsFunction())
                    {
                        AddError(
                            CompilerErrors.UseProcAsFunction(),
                            item.location);
                        continue;
                    }

                    CheckFactArguments(methInfo.GetParameters(), item.factArguments);
                    CorrectCommandArgument(item.commandIndex, GetMethodRefNumber(ref methN));
                }
            }
        }
        
        protected override void VisitModuleVariable(VariableDefinitionNode varNode)
        {
            var symbolicName = varNode.Name;
            var annotations = GetAnnotations(varNode).ToList();
            var binding = _ctx.DefineVariable(symbolicName);
            _module.VariableRefs.Add(binding);

            var fieldBuilder = BslFieldBuilder.Create()
                .Name(symbolicName)
                .SetAnnotations(annotations)
                .SetDispatchingIndex(binding.MemberNumber)
                .DeclaringType(_module.ClassType);
            
            if (varNode.IsExported)
            {
                fieldBuilder.IsExported(true);
                var propertyView = BslPropertyBuilder.Create()
                    .Name(symbolicName)
                    .IsExported(true)
                    .DeclaringType(_module.ClassType)
                    .SetAnnotations(annotations)
                    .SetDispatchingIndex(binding.MemberNumber);
                
                _module.Properties.Add(propertyView.Build());
            }
            
            _module.Fields.Add(fieldBuilder.Build());
        }

        protected override void VisitModuleBody(BslSyntaxNode child)
        {
            if (child.Children.Count == 0)
                return;

            var entry = _module.Code.Count;
            _ctx.PushScope(new SymbolScope());

            try
            {
                VisitCodeBlock(child.Children[0]);
            }
            catch
            {
                _ctx.PopScope();
                throw;
            }

            var localCtx = _ctx.PopScope();
            
            var topIdx = _ctx.TopIndex();

            if (entry != _module.Code.Count)
            {
                var methodInfo = NewMethod()
                    .Name(IExecutableModule.BODY_METHOD_NAME)
                    .DeclaringType(_module.ClassType)
                    .SetDispatchingIndex(_module.Methods.Count)
                    .Build();
                
                methodInfo.SetRuntimeParameters(entry, GetVariableNames(localCtx));
                
                var entryRefNumber = _module.MethodRefs.Count;
                var bodyBinding = new SymbolBinding
                {
                    ScopeNumber = topIdx,
                    MemberNumber = _module.Methods.Count
                };
                
                _module.Methods.Add(methodInfo);
                _module.MethodRefs.Add(bodyBinding);
                _module.EntryMethodIndex = entryRefNumber;
            }
        }

        private static string[] GetVariableNames(SymbolScope localCtx)
        {
            return localCtx.Variables.Select(v => v.Name).ToArray();

        }

        protected override void VisitMethod(MethodNode methodNode)
        {
            var signature = methodNode.Signature;
            var methodBuilder = NewMethod();

            methodBuilder.Name(signature.MethodName)
                .ReturnType(signature.IsFunction ? typeof(BslValue) : typeof(void))
                .IsExported(signature.IsExported)
                .SetDispatchingIndex(_ctx.GetScope(_ctx.TopIndex()).Methods.Count)
                .SetAnnotations(GetAnnotationAttributes(methodNode));
            
            var methodCtx = new SymbolScope();
            
            foreach (var paramNode in signature.GetParameters())
            {
                var parameter = methodBuilder.NewParameter()
                    .Name(paramNode.Name)
                    .ByValue(paramNode.IsByValue)
                    .SetAnnotations(GetAnnotationAttributes(paramNode));
                
                if (paramNode.HasDefaultValue)
                {
                    var constDef = CreateConstDefinition(paramNode.DefaultValue);
                    var defValueIndex = GetConstNumber(constDef);
                    
                    parameter
                        .DefaultValue(_module.Constants[defValueIndex])
                        .CompileTimeBslConstant(defValueIndex);
                }
                
                methodCtx.DefineVariable(new LocalVariableSymbol(paramNode.Name));
            }
            
            _ctx.PushScope(methodCtx);
            var entryPoint = _module.Code.Count;
            try
            {
                VisitMethodBody(methodNode);
            }
            finally
            {
                _ctx.PopScope();
            }

            var methodInfo = methodBuilder.Build();
            methodInfo.SetRuntimeParameters(entryPoint, GetVariableNames(methodCtx));
            
            SymbolBinding binding;
            try
            {
                binding = _ctx.DefineMethod(methodInfo);
            }
            catch (CompilerException)
            {
                AddError(LocalizedErrors.DuplicateMethodDefinition(signature.MethodName), signature.Location);
                binding = default;
            }
            _module.MethodRefs.Add(binding);
            _module.Methods.Add(methodInfo);
        }

        protected override void VisitMethodBody(MethodNode methodNode)
        {
            var codeStart = _module.Code.Count;
            
            base.VisitMethodBody(methodNode);
            
            if (methodNode.Signature.IsFunction)
            {
                var undefConst = new ConstDefinition()
                {
                    Type = DataType.Undefined,
                    Presentation = "Неопределено"
                };

                AddCommand(OperationCode.PushConst, GetConstNumber(undefConst));
            }
            
            var codeEnd = _module.Code.Count;
            
            AddCommand(OperationCode.Return);

            // заменим Return на Jmp <сюда>
            for (var i = codeStart; i < codeEnd; i++)
            {
                if (_module.Code[i].Code == OperationCode.Return)
                {
                    _module.Code[i] = new Command() { Code = OperationCode.Jmp, Argument = codeEnd };
                }
            }
        }

        protected override void VisitMethodVariable(MethodNode method, VariableDefinitionNode variableDefinition)
        {
            _ctx.DefineVariable(variableDefinition.Name);
        }

        protected override void VisitStatement(BslSyntaxNode statement)
        {
            if(statement.Kind != NodeKind.TryExcept)
                AddLineNumber(statement.Location.LineNumber);

            base.VisitStatement(statement);
        }

        protected override void VisitWhileNode(WhileLoopNode node)
        {
            var conditionIndex = _module.Code.Count;
            var loopRecord = NestedLoopInfo.New();
            loopRecord.startPoint = conditionIndex;
            _nestedLoops.Push(loopRecord);
            base.VisitExpression(node.Children[0]);
            var jumpFalseIndex = AddCommand(OperationCode.JmpFalse, DUMMY_ADDRESS);
            
            VisitCodeBlock(node.Children[1]);

            AddCommand(OperationCode.Jmp, conditionIndex);
            var endLoop = AddCommand(OperationCode.Nop);
            CorrectCommandArgument(jumpFalseIndex, endLoop);
            CorrectBreakStatements(_nestedLoops.Pop(), endLoop);
        }

        protected override void VisitForEachLoopNode(ForEachLoopNode node)
        {
            VisitIteratorExpression(node.CollectionExpression);
            AddCommand(OperationCode.PushIterator);
            
            var loopBegin = AddLineNumber(node.Location.LineNumber);
            AddCommand(OperationCode.IteratorNext);
            var condition = AddCommand(OperationCode.JmpFalse, DUMMY_ADDRESS);
            
            VisitIteratorLoopVariable(node.IteratorVariable);
            
            var loopRecord = NestedLoopInfo.New();
            loopRecord.startPoint = loopBegin;
            _nestedLoops.Push(loopRecord);
            
            VisitIteratorLoopBody(node.LoopBody);
            
            AddCommand(OperationCode.Jmp, loopBegin);
            
            VisitBlockEnd(node.EndLocation);
            
            var indexLoopEnd = AddCommand(OperationCode.StopIterator);
            CorrectCommandArgument(condition, indexLoopEnd);
            CorrectBreakStatements(_nestedLoops.Pop(), indexLoopEnd);
        }

        protected override void VisitForLoopNode(ForLoopNode node)
        {
            var initializer = node.InitializationClause;
            var counter = (TerminalNode) initializer.Children[0];
            VisitExpression(initializer.Children[1]);
            VisitVariableWrite(counter);
            
            VisitExpression(node.UpperLimitExpression);
            
            AddCommand(OperationCode.MakeRawValue);
            AddCommand(OperationCode.PushTmp);

            var jmpIndex = AddCommand(OperationCode.Jmp, DUMMY_ADDRESS);
            var indexLoopBegin = AddLineNumber(node.Location.LineNumber);

            // increment
            VisitVariableRead(counter);
            AddCommand(OperationCode.Inc);
            VisitVariableWrite(counter);

            var counterIndex = PushVariable(counter.GetIdentifier());
            CorrectCommandArgument(jmpIndex, counterIndex);
            var conditionIndex = AddCommand(OperationCode.JmpCounter, DUMMY_ADDRESS);

            var loopRecord = NestedLoopInfo.New();
            loopRecord.startPoint = indexLoopBegin;
            _nestedLoops.Push(loopRecord);

            VisitCodeBlock(node.LoopBody);
            VisitBlockEnd(node.EndLocation);

            // jmp to start
            AddCommand(OperationCode.Jmp, indexLoopBegin);

            var indexLoopEnd = AddCommand(OperationCode.PopTmp, 1);
            CorrectCommandArgument(conditionIndex, indexLoopEnd);
            CorrectBreakStatements(_nestedLoops.Pop(), indexLoopEnd);
        }

        protected override void VisitBreakNode(LineMarkerNode node)
        {
            ExitTryBlocks();
            var loopInfo = _nestedLoops.Peek();
            var idx = AddCommand(OperationCode.Jmp, DUMMY_ADDRESS);
            loopInfo.breakStatements.Add(idx);
        }
        
        protected override void VisitContinueNode(LineMarkerNode node)
        {
            ExitTryBlocks();
            var loopInfo = _nestedLoops.Peek();
            AddCommand(OperationCode.Jmp, loopInfo.startPoint);
        }

        protected override void VisitReturnNode(BslSyntaxNode node)
        {
            if (node.Children.Count > 0)
            {
                VisitExpression(node.Children[0]);
                AddCommand(OperationCode.MakeRawValue);
            }
            
            AddCommand(OperationCode.Return);
        }

        protected override void VisitRaiseNode(BslSyntaxNode node)
        {
            int arg = -1;
            if (node.Children.Any())
            {
                VisitExpression(node.Children[0]);
                arg = 0;
            }

            AddCommand(OperationCode.RaiseException, arg);
        }

        protected override void VisitIfNode(ConditionNode node)
        {
            var exitIndices = new List<int>();
            VisitIfExpression(node.Expression);
            
            var jumpFalseIndex = AddCommand(OperationCode.JmpFalse, DUMMY_ADDRESS);

            VisitIfTruePart(node.TruePart);
            exitIndices.Add(AddCommand(OperationCode.Jmp, DUMMY_ADDRESS));

            bool hasAlternativeBranches = false;
            
            foreach (var alternative in node.GetAlternatives())
            {
                CorrectCommandArgument(jumpFalseIndex, _module.Code.Count);
                if (alternative is ConditionNode elif)
                {
                    AddLineNumber(alternative.Location.LineNumber);
                    VisitIfExpression(elif.Expression);
                    jumpFalseIndex = AddCommand(OperationCode.JmpFalse, DUMMY_ADDRESS);
                    VisitIfTruePart(elif.TruePart);
                    exitIndices.Add(AddCommand(OperationCode.Jmp, DUMMY_ADDRESS));
                }
                else
                {
                    hasAlternativeBranches = true;
                    CorrectCommandArgument(jumpFalseIndex, _module.Code.Count);
                    AddLineNumber(alternative.Location.LineNumber, CodeGenerationFlags.CodeStatistics);
                    VisitCodeBlock(alternative);
                }
            }

            int exitIndex = AddLineNumber(node.EndLocation.LineNumber);

            if (!hasAlternativeBranches)
            {
                CorrectCommandArgument(jumpFalseIndex, exitIndex);
            }
            
            foreach (var indexToWrite in exitIndices)
            {
                CorrectCommandArgument(indexToWrite, exitIndex);
            }
        }

        protected override void VisitBlockEnd(in CodeRange endLocation)
        {
            AddLineNumber(
                endLocation.LineNumber,
                CodeGenerationFlags.CodeStatistics | CodeGenerationFlags.DebugCode);
        }

        private void CorrectBreakStatements(NestedLoopInfo nestedLoopInfo, int endLoopIndex)
        {
            foreach (var breakCmdIndex in nestedLoopInfo.breakStatements)
            {
                CorrectCommandArgument(breakCmdIndex, endLoopIndex);
            }
        }

        protected override void VisitAssignment(BslSyntaxNode assignment)
        {
            var left = assignment.Children[0];
            var right = assignment.Children[1];
            if (left is TerminalNode term)
            {
                VisitExpression(right);
                VisitVariableWrite(term);
            }
            else
            {
                VisitReferenceRead(left);
                VisitExpression(right);
                AddCommand(OperationCode.AssignRef);
            }
        }

        protected override void VisitResolveProperty(TerminalNode operand)
        {
            ResolveProperty(operand.GetIdentifier());
        }

        protected override void VisitVariableRead(TerminalNode node)
        {
            try
            {
                PushVariable(node.GetIdentifier());
            }
            catch (SymbolNotFoundException e)
            {
                AddError(LocalizedErrors.SymbolNotFound(e.Symbol), node.Location);
            }
        }

        protected override void VisitVariableWrite(TerminalNode node)
        {
            var identifier = node.GetIdentifier();
            var hasVar = _ctx.TryGetVariable(identifier, out var varBinding);
            if (hasVar)
            {
                if (varBinding.binding.ScopeNumber == _ctx.TopIndex())
                {
                    AddCommand(OperationCode.LoadLoc, varBinding.binding.MemberNumber);
                }
                else
                {
                    var num = GetVariableRefNumber(ref varBinding.binding);
                    AddCommand(OperationCode.LoadVar, num);
                }
            }
            else
            {
                // can create variable
                var binding = _ctx.DefineVariable(identifier);
                AddCommand(OperationCode.LoadLoc, binding.MemberNumber);
            }
        }

        protected override void VisitObjectFunctionCall(BslSyntaxNode node)
        {
            ResolveObjectMethod(node, true);
        }

        protected override void VisitObjectProcedureCall(BslSyntaxNode node)
        {
            ResolveObjectMethod(node, false);
        }

        protected override void VisitIndexExpression(BslSyntaxNode operand)
        {
            base.VisitIndexExpression(operand);
            AddCommand(OperationCode.PushIndexed);
        }

        protected override void VisitGlobalFunctionCall(CallNode node)
        {
            if (LanguageDef.IsBuiltInFunction(node.Identifier.Lexem.Token))
            {
                BuiltInFunctionCall(node);
            }
            else
            {
                GlobalCall(node, true);
            }
        }

        private void BuiltInFunctionCall(CallNode node)
        {
            var funcId = BuiltInFunctionCode(node.Identifier.Lexem.Token);

            var argsPassed = node.ArgumentList.Children.Count;
            PushArgumentsList(node.ArgumentList);
            if (funcId == OperationCode.Min || funcId == OperationCode.Max)
            {
                if (argsPassed == 0)
                    AddError(CompilerErrors.TooFewArgumentsPassed(), node.ArgumentList.Location);
            }
            else
            {
                var parameters = BuiltinFunctions.ParametersInfo(funcId);
                CheckFactArguments(parameters, node.ArgumentList);
            }

            AddCommand(funcId, argsPassed);
        }

        private void CheckFactArguments(ParameterInfo[] parameters, BslSyntaxNode argList)
        {
            var argsPassed = argList.Children.Count;
            if (argsPassed > parameters.Length)
            {
                AddError(CompilerErrors.TooManyArgumentsPassed(), argList.Location);
                return;
            }
            
            if (parameters.Skip(argsPassed).Any(param => !param.HasDefaultValue))
            {
                AddError(CompilerErrors.TooFewArgumentsPassed(), argList.Location);
            }
        }

        private void CheckFactArguments(BslMethodInfo method, BslSyntaxNode argList)
        {
            CheckFactArguments(method.GetParameters(), argList);
        }
        
        protected override void VisitGlobalProcedureCall(CallNode node)
        {
            if (LanguageDef.IsBuiltInFunction(node.Identifier.Lexem.Token))
            {   
                AddError(LocalizedErrors.UseBuiltInFunctionAsProcedure(), node.Location);
                return;
            }
            GlobalCall(node, false);
        }

        private void ResolveObjectMethod(BslSyntaxNode callNode, bool asFunction)
        {
            var name = callNode.Children[0];
            var args = callNode.Children[1];
            
            Debug.Assert(name != null);
            Debug.Assert(args != null);
            
            PushCallArguments(args);
            
            var cDef = new ConstDefinition();
            cDef.Type = DataType.String;
            cDef.Presentation = name.GetIdentifier();
            int lastIdentifierConst = GetConstNumber(cDef);
            
            if (asFunction)
                AddCommand(OperationCode.ResolveMethodFunc, lastIdentifierConst);
            else
                AddCommand(OperationCode.ResolveMethodProc, lastIdentifierConst);
        }
        
        private void ResolveProperty(string identifier)
        {
            var cDef = new ConstDefinition();
            cDef.Type = DataType.String;
            cDef.Presentation = identifier;
            var identifierConstIndex = GetConstNumber(cDef);
            AddCommand(OperationCode.ResolveProp, identifierConstIndex);
        }

        private int PushVariable(string identifier)
        {
            var varNum = _ctx.GetVariable(identifier);
            if (varNum.type == SymbolType.ContextProperty)
            {
                return PushPropertyReference(varNum.binding);
            }
            else
            {
                return PushSimpleVariable(varNum.binding);
            }
        }

        private int PushSimpleVariable(SymbolBinding binding)
        {
            if (binding.ScopeNumber == _ctx.TopIndex())
            {
                return AddCommand(OperationCode.PushLoc, binding.MemberNumber);
            }
            else
            {
                var idx = GetVariableRefNumber(ref binding);
                return AddCommand(OperationCode.PushVar, idx);
            }
        }

        private int PushPropertyReference(SymbolBinding binding)
        {
            var idx = GetVariableRefNumber(ref binding);

            return AddCommand(OperationCode.PushRef, idx);
        }

        private void GlobalCall(CallNode call, bool asFunction)
        {
            var identifierNode = call.Identifier;
            var argList = call.ArgumentList;
            
            Debug.Assert(identifierNode != null);
            Debug.Assert(argList != null);
            
            var identifier = identifierNode.Lexem.Content;
            
            var hasMethod = _ctx.TryGetMethod(identifier, out var methBinding);
            if (hasMethod)
            {
                var scope = _ctx.GetScope(methBinding.ScopeNumber);

                var methInfo = scope.Methods[methBinding.MemberNumber].Method;
                if (asFunction && !methInfo.IsFunction())
                {
                    AddError(CompilerErrors.UseProcAsFunction(), identifierNode.Location);
                    return;
                }

                PushCallArguments(argList);
                CheckFactArguments(methInfo, argList);

                if (asFunction)
                    AddCommand(OperationCode.CallFunc, GetMethodRefNumber(ref methBinding));
                else
                    AddCommand(OperationCode.CallProc, GetMethodRefNumber(ref methBinding)); 
            }
            else
            {
                // can be defined later
                var forwarded = new ForwardedMethodDecl();
                forwarded.identifier = identifier;
                forwarded.asFunction = asFunction;
                forwarded.location = identifierNode.Location;
                forwarded.factArguments = argList;

                PushCallArguments(call.ArgumentList);
                
                var opCode = asFunction ? OperationCode.CallFunc : OperationCode.CallProc;
                forwarded.commandIndex = AddCommand(opCode, DUMMY_ADDRESS);
                _forwardedMethods.Add(forwarded);
            }
        }

        private void PushCallArguments(BslSyntaxNode argList)
        {
            PushArgumentsList(argList);
            AddCommand(OperationCode.ArgNum, argList.Children.Count);
        }

        private void PushArgumentsList(BslSyntaxNode argList)
        {
            for (int i = 0; i < argList.Children.Count; i++)
            {
                var passedArg = argList.Children[i];
                VisitCallArgument(passedArg);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void VisitCallArgument(BslSyntaxNode passedArg)
        {
            if (passedArg.Children.Count > 0)
            {
                VisitExpression(passedArg.Children[0]);
            }
            else
            {
                AddCommand(OperationCode.PushDefaultArg);
            }
        }

        protected override void VisitTernaryOperation(BslSyntaxNode expression)
        {
            VisitExpression(expression.Children[0]);
            AddCommand(OperationCode.MakeBool);
            var addrOfCondition = AddCommand(OperationCode.JmpFalse, DUMMY_ADDRESS);

            VisitExpression(expression.Children[1]); // построили true-part

            var endOfTruePart = AddCommand(OperationCode.Jmp, DUMMY_ADDRESS); // уход в конец оператора
            
            CorrectCommandArgument(addrOfCondition, _module.Code.Count); // отметили, куда переходить по false
            VisitExpression(expression.Children[2]); // построили false-part
            
            CorrectCommandArgument(endOfTruePart, _module.Code.Count);
        }

        protected override void VisitUnaryOperation(UnaryOperationNode unaryOperationNode)
        {
            var child = unaryOperationNode.Children[0];
            VisitExpression(child);
            var opCode = TokenToOperationCode(unaryOperationNode.Operation);
            AddCommand(opCode);
        }
        
        protected override void VisitBinaryOperation(BinaryOperationNode binaryOperationNode)
        {
            if (LanguageDef.IsLogicalBinaryOperator(binaryOperationNode.Operation))
            {
                VisitExpression(binaryOperationNode.Children[0]);
                var logicalCmdIndex = AddCommand(TokenToOperationCode(binaryOperationNode.Operation));
                VisitExpression(binaryOperationNode.Children[1]);
                AddCommand(OperationCode.MakeBool);
                CorrectCommandArgument(logicalCmdIndex, _module.Code.Count - 1);
            }
            else
            {
                VisitExpression(binaryOperationNode.Children[0]);
                VisitExpression(binaryOperationNode.Children[1]);
                AddCommand(TokenToOperationCode(binaryOperationNode.Operation));
            }
        }

        protected override void VisitTryExceptNode(TryExceptNode node)
        {
            var beginTryIndex = AddCommand(OperationCode.BeginTry, DUMMY_ADDRESS);
            VisitTryBlock(node.TryBlock);
            var jmpIndex = AddCommand(OperationCode.Jmp, DUMMY_ADDRESS);

            var beginHandler = AddLineNumber(
                node.ExceptBlock.Location.LineNumber,
                CodeGenerationFlags.CodeStatistics);

            CorrectCommandArgument(beginTryIndex, beginHandler);

            VisitExceptBlock(node.ExceptBlock);

            var endIndex = AddLineNumber(node.EndLocation.LineNumber,
                CodeGenerationFlags.CodeStatistics | CodeGenerationFlags.DebugCode);
            
            AddCommand(OperationCode.EndTry);
            CorrectCommandArgument(jmpIndex, endIndex);
        }

        protected override void VisitTryBlock(CodeBatchNode node)
        {
            PushTryNesting();
            base.VisitTryBlock(node);
            PopTryNesting();
        }

        protected override void VisitExecuteStatement(BslSyntaxNode node)
        {
            base.VisitExecuteStatement(node);
            AddCommand(OperationCode.Execute);
        }

        protected override void VisitHandlerOperation(BslSyntaxNode node)
        {
            var (srcValue, eventName) = SplitExpressionAndName(node.Children[0]);
            VisitExpression(srcValue);
            VisitConstant(eventName);

            var handlerNode = node.Children[1];
            if(handlerNode.Kind == NodeKind.Identifier)
                VisitConstant((TerminalNode)handlerNode);
            else
            {
                var (handler, procedureName) = SplitExpressionAndName(node.Children[1]);
                VisitExpression(handler);
                VisitConstant(procedureName);
            }

            AddCommand(node.Kind == NodeKind.AddHandler ? OperationCode.AddHandler : OperationCode.RemoveHandler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (BslSyntaxNode, Lexem) SplitExpressionAndName(BslSyntaxNode node)
        {
            var term = (TerminalNode) node.Children[1];
            var lex = term.Lexem;
            lex.Type = LexemType.StringLiteral;
            return (node.Children[0], lex);
        }

        protected override void VisitNewObjectCreation(NewObjectNode node)
        {
            if (node.IsDynamic)
            {
                MakeNewObjectDynamic(node);
            }
            else
            {
                MakeNewObjectStatic(node);
            }
        }
        
        private void MakeNewObjectDynamic(NewObjectNode node)
        {
            VisitExpression(node.TypeNameNode);

            var argsPassed = node.ConstructorArguments.Children.Count;
            if (argsPassed == 1)
            {
                PushArgumentsList(node.ConstructorArguments);
            }
            else if (argsPassed > 1)
            {
                AddError(CompilerErrors.TooManyArgumentsPassed(), node.ConstructorArguments.Location);
            }

            AddCommand(OperationCode.NewFunc, argsPassed);
        }
        
        private void MakeNewObjectStatic(NewObjectNode node)
        {
            var cDef = new ConstDefinition()
            {
                Type = DataType.String,
                Presentation = node.TypeNameNode.GetIdentifier()
            };
            AddCommand(OperationCode.PushConst, GetConstNumber(cDef));

            var callArgs = 0;
            if (node.ConstructorArguments != default)
            {
                PushArgumentsList(node.ConstructorArguments);
                callArgs = node.ConstructorArguments.Children.Count;
            }

            AddCommand(OperationCode.NewInstance, callArgs);
        }

        private void ExitTryBlocks()
        {
            var tryBlocks = _nestedLoops.Peek().tryNesting;
            if (tryBlocks > 0)
                AddCommand(OperationCode.ExitTry, tryBlocks);
        }

        private void PushTryNesting()
        {
            if (_nestedLoops.Count > 0)
            {
                _nestedLoops.Peek().tryNesting++;
            }
        }
        
        private void PopTryNesting()
        {
            if (_nestedLoops.Count > 0)
            {
                _nestedLoops.Peek().tryNesting--;
            }
        }
        
        private void CorrectCommandArgument(int index, int newArgument)
        {
            var cmd = _module.Code[index];
            cmd.Argument = newArgument;
            _module.Code[index] = cmd;
        }

        private static OperationCode TokenToOperationCode(Token stackOp)
        {
            OperationCode opCode;
            switch (stackOp)
            {
                case Token.Equal:
                    opCode = OperationCode.Equals;
                    break;
                case Token.NotEqual:
                    opCode = OperationCode.NotEqual;
                    break;
                case Token.Plus:
                    opCode = OperationCode.Add;
                    break;
                case Token.Minus:
                    opCode = OperationCode.Sub;
                    break;
                case Token.Multiply:
                    opCode = OperationCode.Mul;
                    break;
                case Token.Division:
                    opCode = OperationCode.Div;
                    break;
                case Token.Modulo:
                    opCode = OperationCode.Mod;
                    break;
                case Token.UnaryPlus:
                    opCode = OperationCode.Number;
                    break;
                case Token.UnaryMinus:
                    opCode = OperationCode.Neg;
                    break;
                case Token.And:
                    opCode = OperationCode.And;
                    break;
                case Token.Or:
                    opCode = OperationCode.Or;
                    break;
                case Token.Not:
                    opCode = OperationCode.Not;
                    break;
                case Token.LessThan:
                    opCode = OperationCode.Less;
                    break;
                case Token.LessOrEqual:
                    opCode = OperationCode.LessOrEqual;
                    break;
                case Token.MoreThan:
                    opCode = OperationCode.Greater;
                    break;
                case Token.MoreOrEqual:
                    opCode = OperationCode.GreaterOrEqual;
                    break;
                case Token.AddHandler:
                    opCode = OperationCode.AddHandler;
                    break;
                case Token.RemoveHandler:
                    opCode = OperationCode.RemoveHandler;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return opCode;
        }

        protected override void VisitConstant(TerminalNode node)
        {
            var cDef = CreateConstDefinition(node.Lexem);
            var num = GetConstNumber(cDef);
            AddCommand(OperationCode.PushConst, num);
        }
        
        private void VisitConstant(in Lexem constant)
        {
            var cDef = CreateConstDefinition(constant);
            var num = GetConstNumber(cDef);
            AddCommand(OperationCode.PushConst, num);
        }

        private IEnumerable<object> GetAnnotationAttributes(AnnotatableNode node)
        {
            var mappedAnnotations = new List<object>();
            foreach (var annotation in node.Annotations)
            {
                var anno = new BslAnnotationAttribute(annotation.Name);
                anno.SetParameters(GetAnnotationParameters(annotation));
                mappedAnnotations.Add(anno);
            }

            return mappedAnnotations;
        }
        
        private IEnumerable<BslAnnotationParameter> GetAnnotationParameters(AnnotationNode node)
        {
            return node.Children.Cast<AnnotationParameterNode>()
                .Select(MakeAnnotationParameter)
                .ToList();
        }

        private BslAnnotationParameter MakeAnnotationParameter(AnnotationParameterNode param)
        {
            BslAnnotationParameter result;
            if (param.Value.Type != LexemType.NotALexem)
            {
                var constDef = CreateConstDefinition(param.Value);
                var constNumber = GetConstNumber(constDef);
                var runtimeValue = _module.Constants[constNumber];
                result = new BslAnnotationParameter(param.Name, runtimeValue)
                {
                    ConstantValueIndex = constNumber
                };
            }
            else
            {
                result = new BslAnnotationParameter(param.Name, null);
            }

            return result;
        }

        private IEnumerable<BslAnnotationAttribute> GetAnnotations(AnnotatableNode parent)
        {
            return parent.Annotations.Select(a =>
                new BslAnnotationAttribute(
                    a.Name,
                    GetAnnotationParameters(a)));
        }

        private static BslMethodBuilder<MachineMethodInfo> NewMethod() =>
            new BslMethodInfoFactory<MachineMethodInfo>(() => new MachineMethodInfo())
                .NewMethod();
        
        private static ConstDefinition CreateConstDefinition(in Lexem lex)
        {
            DataType constType = DataType.Undefined;
            switch (lex.Type)
            {
                case LexemType.BooleanLiteral:
                    constType = DataType.Boolean;
                    break;
                case LexemType.DateLiteral:
                    constType = DataType.Date;
                    break;
                case LexemType.NumberLiteral:
                    constType = DataType.Number;
                    break;
                case LexemType.StringLiteral:
                    constType = DataType.String;
                    break;
                case LexemType.NullLiteral:
                    constType = DataType.GenericValue;
                    break;
            }

            ConstDefinition cDef = new ConstDefinition()
            {
                Type = constType,
                Presentation = lex.Content
            };
            return cDef;
        }
        
        private int GetConstNumber(in ConstDefinition cDef)
        {
            var idx = _constMap.IndexOf(cDef);
            if (idx < 0)
            {
                idx = _constMap.Count;
                _constMap.Add(cDef);
                _module.Constants.Add((BslPrimitiveValue)ValueFactory.Parse(cDef.Presentation, cDef.Type));
            }
            return idx;
        }

        private int GetMethodRefNumber(ref SymbolBinding methodBinding)
        {
            var idx = _module.MethodRefs.IndexOf(methodBinding);
            if (idx < 0)
            {
                idx = _module.MethodRefs.Count;
                _module.MethodRefs.Add(methodBinding);
            }
            return idx;
        }

        private int GetVariableRefNumber(ref SymbolBinding binding)
        {
            var idx = _module.VariableRefs.IndexOf(binding);
            if (idx < 0)
            {
                idx = _module.VariableRefs.Count;
                _module.VariableRefs.Add(binding);
            }

            return idx;
        }
        
        private void AddError(CodeError error, in CodeRange location)
        {
            error.Position = MakeCodePosition(location);
            _errorSink.AddError(error);
        }
        
        private void AddError(CodeError error)
        {
            _errorSink.AddError(error);
        }

        private ErrorPositionInfo MakeCodePosition(CodeRange range)
        {
            return new ErrorPositionInfo
            {
                Code = _sourceCode.GetCodeLine(range.LineNumber),
                LineNumber = range.LineNumber,
                ColumnNumber = range.ColumnNumber,
                ModuleName = _sourceCode.Name
            };
        }

        private int AddCommand(OperationCode code, int arg = 0)
        {
            var addr = _module.Code.Count;
            _module.Code.Add(new Command() { Code = code, Argument = arg });
            return addr;
        }
        
        private int AddLineNumber(int linenum, CodeGenerationFlags emitConditions = CodeGenerationFlags.Always)
        {
            var addr = _module.Code.Count;
            bool emit = emitConditions == CodeGenerationFlags.Always || ExtraCodeConditionsMet(emitConditions);
            if (emit)
            {
                _module.Code.Add(new Command() { Code = OperationCode.LineNum, Argument = linenum });
            }
            return addr;
        }

        private bool ExtraCodeConditionsMet(CodeGenerationFlags emitConditions)
        {
            return (((int)ProduceExtraCode) & (int)emitConditions) != 0;
        }
        
        #region Static cache
        
        private static readonly Dictionary<Token, OperationCode> _tokenToOpCode;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static OperationCode BuiltInFunctionCode(Token token)
        {
            return _tokenToOpCode[token];
        }
        
        static StackMachineCodeGenerator()
        {
            _tokenToOpCode = new Dictionary<Token, OperationCode>();

            var tokens  = LanguageDef.BuiltInFunctions();
            var opCodes = BuiltinFunctions.GetOperationCodes();

            Debug.Assert(tokens.Length == opCodes.Length);
            for (int i = 0; i < tokens.Length; i++)
            {
                _tokenToOpCode.Add(tokens[i], opCodes[i]);
            }
        }
        
        #endregion
    }
}