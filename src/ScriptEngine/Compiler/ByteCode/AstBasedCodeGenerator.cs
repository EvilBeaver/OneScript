/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler.ByteCode
{
    internal partial class AstBasedCodeGenerator
    {
        private ModuleImage _module;
        private ICompilerContext _ctx;
        private List<CompilerException> _errors = new List<CompilerException>();
        private ModuleInformation _moduleInfo;

        private readonly List<ForwardedMethodDecl> _forwardedMethods = new List<ForwardedMethodDecl>();

        public AstBasedCodeGenerator(ICompilerContext context)
        {
            _ctx = context;
        }

        public bool ThrowErrors { get; set; }

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
            throw new NotImplementedException();
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
                WriteStatement(statement);
            }
        }

        private void WriteStatement(AstNodeBase statement)
        {
            var nonTerminal = statement as NonTerminalNode;
            if (statement.Kind == NodeKind.Assignment)
            {
                PushDereferenceTarget(nonTerminal.Children[0]);
                PushExpressionResult(nonTerminal.Children[1]);
            }
            else if (statement.Kind == NodeKind.Call)
            {
                PushDereferenceTarget(nonTerminal.Children[0]);
                CallObjectMethod(nonTerminal.Children[1]);
            }
            else if (statement.Kind == NodeKind.DereferenceOperation)
            {
                
            }
            else
            {
                
            }
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

        private CodePositionInfo MakeCodePosition(CodeRange range)
        {
            return range.ToCodePosition(_moduleInfo);
        }
    }
}