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
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using MethodInfo = ScriptEngine.Machine.MethodInfo;

namespace ScriptEngine.Compiler.ByteCode
{
    internal partial class AstBasedCodeGenerator
    {
        private ModuleImage _module;
        private ICompilerContext _ctx;
        private List<CompilerException> _errors = new List<CompilerException>();
        private ModuleInformation _moduleInfo;

        private Action<NonTerminalNode>[] _nodeWriters;

        private readonly List<ForwardedMethodDecl> _forwardedMethods = new List<ForwardedMethodDecl>();

        public AstBasedCodeGenerator(ICompilerContext context)
        {
            _ctx = context;
            _module = new ModuleImage();
            _nodeWriters = new Action<NonTerminalNode>[
                typeof(NodeKind).GetFields(BindingFlags.Static|BindingFlags.Public).Length
            ];
            _nodeWriters[NodeKind.WhileLoop] = WriteWhileLoop;
        }

        public bool ThrowErrors { get; set; }
        
        public CodeGenerationFlags ProduceExtraCode { get; set; }

        public IReadOnlyList<CompilerException> Errors => _errors;
        
        public ModuleImage CreateImage(NonTerminalNode moduleNode, ModuleInformation moduleInfo)
        {
            if (moduleNode.Kind != NodeKind.Module)
            {
                throw new ArgumentException($"Node must be a Module node");
            }

            _moduleInfo = moduleInfo;

            return CreateImageInternal(moduleNode);
        }

        private ModuleImage CreateImageInternal(NonTerminalNode moduleNode)
        {
            foreach (var child in moduleNode.Children.Cast<NonTerminalNode>())
            {
                switch (child.Kind)
                {
                    case NodeKind.VariablesSection:
                        child.Children
                            .ForEach(x => WriteModuleVariable((VariableDefinitionNode)x));
                        break;
                    case NodeKind.MethodsSection:
                        child.Children
                            .ForEach(x => WriteMethod((MethodNode)x));
                        break;
                    case NodeKind.ModuleBody:
                        WriteModuleBody(child);
                        break;
                }
            }

            return _module;
        }

        private void WriteModuleBody(NonTerminalNode child)
        {
            var entry = _module.Code.Count;
            _ctx.PushScope(new SymbolScope());

            try
            {
                WriteCodeBatch(child.Children[0].AsNonTerminal());
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
        
        private void WriteMethod(MethodNode methodNode)
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
            try
            {
                DispatchMethodBody(methodNode);
            }
            finally
            {
                _ctx.PopScope();
            }
        }

        private void DispatchMethodBody(MethodNode methodNode)
        {
            foreach (var node in methodNode
                .Children
                .SkipWhile(x => x.Kind == NodeKind.Annotation || x.Kind == NodeKind.MethodSignature))
            {
                if (node is VariableDefinitionNode variable)
                {
                    _ctx.DefineVariable(variable.Name);
                }
                else if(node.Kind == NodeKind.CodeBatch)
                {
                    WriteCodeBatch((NonTerminalNode)node);
                }
            }
        }

        private void WriteCodeBatch(NonTerminalNode node)
        {
            foreach (var statement in node.Children)
            {
                WriteStatement((NonTerminalNode)statement);
            }
        }

        private void WriteStatement(NonTerminalNode statement)
        {
            AddLineNumber(statement.Location.LineNumber);
            
            if (statement.Kind == NodeKind.Assignment)
            {
                WriteAssignment(statement);
            }
            else if (statement.Kind == NodeKind.Call)
            {
                GlobalCall(statement, false);
            }
            else if (statement.Kind == NodeKind.DereferenceOperation)
            {
                PushReference(statement.Children[0].AsNonTerminal());
                ResolveObjectMethod(statement.Children[1].AsNonTerminal(), false);
            }
            else
            {
                _nodeWriters[statement.Kind](statement);
            }
        }

        private void WriteWhileLoop(NonTerminalNode node)
        {
            var loop = node as WhileLoopNode;
        }

        private void WriteAssignment(NonTerminalNode assignment)
        {
            var left = assignment.Children[0];
            var right = assignment.Children[1];
            if (left is TerminalNode term)
            {
                PushExpressionResult(right);
                BuildLoadVariable(term.Lexem.Content);
            }
            else
            {
                PushReference(left.AsNonTerminal());
                PushExpressionResult(right);
                AddCommand(OperationCode.AssignRef);
            }
        }

        private void PushReference(NonTerminalNode operation)
        {
            switch (operation.Kind)
            {
                case NodeKind.DereferenceOperation:
                {
                    PushAccessTarget(operation.Children[0]);
                    var argument = operation.Children[1];
                    if (argument.Kind == NodeKind.Identifier)
                    {
                        ResolveProperty(argument.GetIdentifier());
                    }
                    else
                    {
                        Debug.Assert(argument.Kind == NodeKind.Call);
                        ResolveObjectMethod(argument.AsNonTerminal(), true);
                    }
                    break;
                }
                case NodeKind.IndexAccess:
                {
                    PushAccessTarget(operation.Children[0]);
                    var argument = operation.Children[1];
                    PushExpressionResult(argument);
                    AddCommand(OperationCode.PushIndexed);
                    break;
                }
                case NodeKind.Call:
                    GlobalCall(operation, true);
                    return;
                default:
                    throw new ApplicationException($"Wrong tree structure: {operation.Kind}/{operation.Location.LineNumber}");
            }
        }

        private void ResolveObjectMethod(NonTerminalNode callNode, bool asFunction)
        {
            var name = callNode.Children[0];
            var args = callNode.Children[1];
            
            Debug.Assert(name != null);
            Debug.Assert(args != null);
            
            var cDef = new ConstDefinition();
            cDef.Type = DataType.String;
            cDef.Presentation = name.GetIdentifier();
            int lastIdentifierConst = GetConstNumber(cDef);
            PushCallArguments(args.AsNonTerminal());
            if (asFunction)
                AddCommand(OperationCode.ResolveMethodFunc, lastIdentifierConst);
            else
                AddCommand(OperationCode.ResolveMethodProc, lastIdentifierConst);
        }
        
        private void PushAccessTarget(AstNodeBase target)
        {
            if (target.Kind == NodeKind.Identifier)
            {
                PushVariable(target.GetIdentifier());
            }
            else
            {
                PushReference(target.AsNonTerminal());
            }
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

        private void BuildLoadVariable(string identifier)
        {
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
        
        private void GlobalCall(NonTerminalNode nonTerminal, bool asFunction)
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
                    PushExpressionResult(passedArg.Children[0]);
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

        private void PushCallArguments(NonTerminalNode argList)
        {
            for (int i = 0; i < argList.Children.Count; i++)
            {
                var passedArg = (NonTerminalNode)argList.Children[i];
                if (passedArg.Children.Count > 0)
                {
                    PushExpressionResult(passedArg.Children[0]);
                }
                else
                {
                    AddCommand(OperationCode.PushDefaultArg);
                }
            }
            
            AddCommand(OperationCode.ArgNum, argList.Children.Count);
        }
        
        private void PushExpressionResult(AstNodeBase expression)
        {
            if (expression is TerminalNode term)
            {
                PushTerminalSymbol(term);
            }
            else
            {
                DispatchExpression(expression.AsNonTerminal());
            }
        }

        private void DispatchExpression(NonTerminalNode expression)
        {
            switch (expression.Kind)
            {
                case NodeKind.BinaryOperation:
                    WriteBinaryOperation((BinaryOperationNode)expression);
                    break;
                case NodeKind.UnaryOperation:
                    WriteUnaryOperaion((UnaryOperationNode)expression);
                    break;
                case NodeKind.TernaryOperator:
                    WriteTernaryOperator(expression);
                    break;
                default:
                    PushReference(expression);
                    break;
            }
        }

        private void WriteTernaryOperator(NonTerminalNode expression)
        {
            throw new NotImplementedException();
        }

        private void WriteUnaryOperaion(UnaryOperationNode unaryOperationNode)
        {
            var child = unaryOperationNode.Children[0];
            PushExpressionResult(child);
            AddCommand(TokenToOperationCode(unaryOperationNode.Operation));
        }
        
        private void WriteBinaryOperation(BinaryOperationNode binaryOperationNode)
        {
            if (LanguageDef.IsLogicalBinaryOperator(binaryOperationNode.Operation))
            {
                PushExpressionResult(binaryOperationNode.Children[0]);
                var logicalCmdIndex = AddCommand(TokenToOperationCode(binaryOperationNode.Operation));
                PushExpressionResult(binaryOperationNode.Children[1]);
                AddCommand(OperationCode.MakeBool);
                CorrectCommandArgument(logicalCmdIndex, _module.Code.Count - 1);
            }
            else
            {
                PushExpressionResult(binaryOperationNode.Children[0]);
                PushExpressionResult(binaryOperationNode.Children[1]);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushTerminalSymbol(TerminalNode term)
        {
            switch (term.Kind)
            {
                case NodeKind.Identifier:
                    PushVariable(term.GetIdentifier());
                    break;
                case NodeKind.Constant:
                    PushConstant(term.Lexem);
                    break;
            }
        }

        private void PushConstant(in Lexem lexem)
        {
            var cDef = CreateConstDefinition(lexem);
            var num = GetConstNumber(cDef);
            AddCommand(OperationCode.PushConst, num);
        }
        
        private void WriteModuleVariable(VariableDefinitionNode variableNode)
        {
            var symbolicName = variableNode.Name;
            var annotations = GetAnnotations(variableNode);
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