/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler.ByteCode
{
    internal partial class AstBasedCodeGenerator : DefaultAstBuilder
    {
        private ModuleImage _module;
        private ICompilerContext _ctx;
        
        private readonly List<ForwardedMethodDecl> _forwardedMethods = new List<ForwardedMethodDecl>();

        public override void AddChild(IAstNode parent, IAstNode child)
        {
            base.AddChild(parent, child);
            OnChildAdded((NonTerminalNode) parent, (AstNodeBase) child);
        }

        private void OnChildAdded(NonTerminalNode parent, AstNodeBase child)
        {
            if (parent.Kind == NodeKind.VariablesSection && child.Kind == NodeKind.VariableDefinition)
            {
                WriteModuleVariable((VariableDefinitionNode)child);
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
        
        public override void HandleParseError(in ParseError error, in Lexem lexem, ILexemGenerator lexer)
        {
            //throw new System.NotImplementedException();
        }

        public override void PreprocessorDirective(ILexemGenerator lexer, ref Lexem lastExtractedLexem)
        {
            throw new System.NotImplementedException();
        }
    }
}