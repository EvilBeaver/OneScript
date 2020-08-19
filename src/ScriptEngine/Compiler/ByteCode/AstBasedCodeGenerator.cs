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
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Language.SyntaxAnalysis.Traversal;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler.ByteCode
{
    public partial class AstBasedCodeGenerator : BslSyntaxWalker
    {
        private ModuleImage _module;
        private ICompilerContext _ctx;
        private List<CompilerException> _errors = new List<CompilerException>();
        private ModuleInformation _moduleInfo;

        private readonly List<ForwardedMethodDecl> _forwardedMethods = new List<ForwardedMethodDecl>();
        private readonly Stack<NestedLoopInfo> _nestedLoops = new Stack<NestedLoopInfo>();

        public AstBasedCodeGenerator(ICompilerContext context)
        {
            _ctx = context;
            _module = new ModuleImage();
        }

        public bool ThrowErrors { get; set; }
        
        public CodeGenerationFlags ProduceExtraCode { get; set; }

        public IReadOnlyList<CompilerException> Errors => _errors;
        
        public ModuleImage CreateImage(BslSyntaxNode moduleNode, ModuleInformation moduleInfo)
        {
            if (moduleNode.Kind != NodeKind.Module)
            {
                throw new ArgumentException($"Node must be a Module node");
            }

            _moduleInfo = moduleInfo;

            return CreateImageInternal(moduleNode);
        }

        private ModuleImage CreateImageInternal(BslSyntaxNode moduleNode)
        {
            VisitModule(moduleNode);
            _module.LoadAddress = _ctx.TopIndex();
            return _module;
        }

        protected override void VisitModuleVariable(VariableDefinitionNode varNode)
        {
            var symbolicName = varNode.Name;
            var annotations = GetAnnotations(varNode);
            var definition = _ctx.DefineVariable(symbolicName);
            _module.VariableRefs.Add(definition);
            _module.Variables.Add(new VariableInfo
            {
                Identifier = symbolicName,
                Annotations = annotations,
                CanGet = true,
                CanSet = true,
                Index = definition.CodeIndex
            });

            if (varNode.IsExported)
            {
                _module.ExportedProperties.Add(new ExportedSymbol
                {
                    SymbolicName = symbolicName,
                    Index = definition.CodeIndex
                });
            }
        }

        protected override void VisitModuleBody(BslSyntaxNode child)
        {
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
                var bodyMethod = new MethodInfo();
                bodyMethod.Name = BODY_METHOD_NAME;
                var descriptor = new MethodDescriptor();
                descriptor.EntryPoint = entry;
                descriptor.Signature = bodyMethod;
                FillVariablesFrame(ref descriptor, localCtx);

                var entryRefNumber = _module.MethodRefs.Count;
                var bodyBinding = new SymbolBinding()
                {
                    ContextIndex = topIdx,
                    CodeIndex = _module.Methods.Count
                };
                _module.Methods.Add(descriptor);
                _module.MethodRefs.Add(bodyBinding);
                _module.EntryMethodIndex = entryRefNumber;
            }
        }

        private static void FillVariablesFrame(ref MethodDescriptor descriptor, SymbolScope localCtx)
        {
            descriptor.Variables = new VariablesFrame();

            for (int i = 0; i < localCtx.VariableCount; i++)
            {
                descriptor.Variables.Add(localCtx.GetVariable(i));
            }
        }
        
        protected override void VisitMethod(MethodNode methodNode)
        {
            var signature = methodNode.Signature;
            if (_ctx.TryGetMethod(signature.MethodName, out _))
            {
                var err = new CompilerException(Locale.NStr($"ru = 'Метод с таким именем уже определен: {signature.MethodName}';"+
                                                            $"en = 'Method is already defined {signature.MethodName}'"));
                AddError(CompilerException.AppendCodeInfo(err, MakeCodePosition(signature.Location)));
                return;
            }
            
            MethodInfo method = new MethodInfo();
            method.Name = signature.MethodName;
            method.IsFunction = signature.IsFunction;
            method.Annotations = GetAnnotations(methodNode);
            method.IsExport = signature.IsExported;
            
            var methodCtx = new SymbolScope();
            var paramsList = new List<ParameterDefinition>();
            foreach (var paramNode in signature.GetParameters())
            {
                var constDef = CreateConstDefinition(paramNode.DefaultValue);
                var p = new ParameterDefinition
                {
                    Annotations = GetAnnotations(paramNode),
                    Name = paramNode.Name,
                    IsByValue = paramNode.IsByValue,
                    HasDefaultValue = paramNode.HasDefaultValue,
                    DefaultValueIndex = GetConstNumber(constDef)
                };
                paramsList.Add(p);
                methodCtx.DefineVariable(p.Name);
            }
            method.Params = paramsList.ToArray();
            
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
            
            var descriptor = new MethodDescriptor();
            descriptor.EntryPoint = entryPoint;
            descriptor.Signature = method;
            FillVariablesFrame(ref descriptor, methodCtx);

            SymbolBinding binding;
            binding = _ctx.DefineMethod(method);
            _module.MethodRefs.Add(binding);
            _module.Methods.Add(descriptor);

            // TODO: deprecate?
            if (signature.IsExported)
            {
                _module.ExportedMethods.Add(new ExportedSymbol()
                {
                    SymbolicName = method.Name,
                    Index = binding.CodeIndex
                });
            }

        }

        protected override void VisitMethodVariable(MethodNode method, VariableDefinitionNode variableDefinition)
        {
            _ctx.DefineVariable(variableDefinition.Name);
        }

        protected override void VisitStatement(BslSyntaxNode statement)
        {
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
                AddError(e, node.Location);
            }
        }

        protected override void VisitVariableWrite(TerminalNode node)
        {
            var identifier = node.GetIdentifier();
            var hasVar = _ctx.TryGetVariable(identifier, out var varBinding);
            if (hasVar)
            {
                if (varBinding.binding.ContextIndex == _ctx.TopIndex())
                {
                    AddCommand(OperationCode.LoadLoc, varBinding.binding.CodeIndex);
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
                AddCommand(OperationCode.LoadLoc, binding.CodeIndex);
            }
        }

        protected override void VisitObjectFunctionCall(BslSyntaxNode node)
        {
            ResolveObjectMethod(node, true);
        }

        protected override void VisitIndexExpression(BslSyntaxNode operand)
        {
            base.VisitIndexExpression(operand);
            AddCommand(OperationCode.PushIndexed);
        }

        protected override void VisitGlobalFunctionCall(BslSyntaxNode node)
        {
            GlobalCall(node, true);
        }

        protected override void VisitGlobalProcedureCall(BslSyntaxNode node)
        {
            GlobalCall(node, false);
        }

        private void ResolveObjectMethod(BslSyntaxNode callNode, bool asFunction)
        {
            var name = callNode.Children[0];
            var args = callNode.Children[1];
            
            Debug.Assert(name != null);
            Debug.Assert(args != null);
            
            var cDef = new ConstDefinition();
            cDef.Type = DataType.String;
            cDef.Presentation = name.GetIdentifier();
            int lastIdentifierConst = GetConstNumber(cDef);
            PushCallArguments(args);
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
            if (binding.ContextIndex == _ctx.TopIndex())
            {
                return AddCommand(OperationCode.PushLoc, binding.CodeIndex);
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

        private void GlobalCall(BslSyntaxNode nonTerminal, bool asFunction)
        {
            var identifierNode = nonTerminal.Children[0] as TerminalNode;
            var argList = nonTerminal.Children[1] as NonTerminalNode;
            
            Debug.Assert(identifierNode != null);
            Debug.Assert(argList != null);
            
            var identifier = identifierNode.Lexem.Content;
            
            var hasMethod = _ctx.TryGetMethod(identifier, out var methBinding);
            if (hasMethod)
            {
                var scope = _ctx.GetScope(methBinding.ContextIndex);

                // dynamic scope checks signatures only at runtime
                if (!scope.IsDynamicScope)
                {
                    var methInfo = scope.GetMethod(methBinding.CodeIndex);
                    if (asFunction && !methInfo.IsFunction)
                    {
                        AddError(CompilerException.UseProcAsFunction(), identifierNode.Location);
                        return;
                    }

                    PushGlobalCallArguments(methInfo, argList);
                }

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
                forwarded.codeLine = identifierNode.Location.LineNumber;
                forwarded.factArguments = argList;

                var opCode = asFunction ? OperationCode.CallFunc : OperationCode.CallProc;
                forwarded.commandIndex = AddCommand(opCode, DUMMY_ADDRESS);
                _forwardedMethods.Add(forwarded);
            }
        }

        private void PushGlobalCallArguments(MethodInfo methInfo, NonTerminalNode argList)
        {
            var parameters = methInfo.Params;
            
            if (argList.Children.Count > parameters.Length)
            {
                throw CompilerException.TooManyArgumentsPassed();
            }

            for (int i = 0; i < argList.Children.Count; i++)
            {
                var passedArg = (NonTerminalNode)argList.Children[i];
                if (passedArg.Children.Count > 0)
                {
                    VisitExpression(passedArg.Children[0]);
                }
                else
                {
                    if (parameters[i].HasDefaultValue)
                    {
                        AddCommand(OperationCode.PushDefaultArg);
                    }
                    else
                    {
                        var exc = new CompilerException(Locale.NStr(
                            $"ru='Параметр {i} метода {methInfo.Name} является обязательным';"+
                            $"en='Parameter {i} of method {methInfo.Name} is required'"));
                        AddError(exc, passedArg.Location);
                    }
                }
            }
            
            if (parameters.Skip(argList.Children.Count).Any(param => !param.HasDefaultValue))
            {
                AddError(CompilerException.TooFewArgumentsPassed(), argList.Location);
            }

            AddCommand(OperationCode.ArgNum, argList.Children.Count);
        }

        private void PushCallArguments(BslSyntaxNode argList)
        {
            for (int i = 0; i < argList.Children.Count; i++)
            {
                var passedArg = argList.Children[i];
                if (passedArg.Children.Count > 0)
                {
                    VisitExpression(passedArg.Children[0]);
                }
                else
                {
                    AddCommand(OperationCode.PushDefaultArg);
                }
            }
            
            AddCommand(OperationCode.ArgNum, argList.Children.Count);
        }
        
        protected override void VisitTernaryOperation(BslSyntaxNode expression)
        {
            throw new NotImplementedException();
        }

        protected override void VisitUnaryOperation(UnaryOperationNode unaryOperationNode)
        {
            var child = unaryOperationNode.Children[0];
            VisitExpression(child);
            AddCommand(TokenToOperationCode(unaryOperationNode.Operation));
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

        private AnnotationDefinition[] GetAnnotations(AnnotatableNode parent)
        {
            var annotations = new List<AnnotationDefinition>();
            foreach (var node in parent.Annotations)
            {
                annotations.Add(new AnnotationDefinition
                {
                    Name = node.Name,
                    Parameters = GetAnnotationParameters(node)
                });
            }

            return annotations.ToArray();
        }

        private AnnotationParameter[] GetAnnotationParameters(AnnotationNode node)
        {
            var parameters = new List<AnnotationParameter>();
            foreach (var param in node.Children.Cast<AnnotationParameterNode>())
            {
                var paramDef = new AnnotationParameter();
                if (param.Name != default)
                {
                    paramDef.Name = param.Name;
                }

                if (param.Value.Type != LexemType.NotALexem)
                {
                    var constDef = CreateConstDefinition(param.Value);
                    paramDef.ValueIndex = GetConstNumber(constDef);
                }
                else
                {
                    paramDef.ValueIndex = AnnotationParameter.UNDEFINED_VALUE_INDEX;
                }
                parameters.Add(paramDef);
            }

            return parameters.ToArray();
        }

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
            var idx = _module.Constants.IndexOf(cDef);
            if (idx < 0)
            {
                idx = _module.Constants.Count;
                _module.Constants.Add(cDef);
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

        private void AddError(CompilerException exc)
        {
            if (ThrowErrors)
                throw exc;

            _errors.Add(exc);
        }
        
        private void AddError(CompilerException error, in CodeRange location)
        {
            CompilerException.AppendCodeInfo(error, MakeCodePosition(location));
            AddError(error);
        }

        private CodePositionInfo MakeCodePosition(CodeRange range)
        {
            return range.ToCodePosition(_moduleInfo);
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
    }
}