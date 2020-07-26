/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Compiler.ByteCode;

namespace OneScript.Language.SyntaxAnalysis
{
    public class DefaultAstBuilder : IAstBuilder
    {
        public NonTerminalNode RootNode { get; private set; }
        
        public bool ThrowOnError { get; set; }
        
        public virtual IAstNode CreateNode(int kind, in Lexem startLexem)
        {
            switch (kind)
            {
                case NodeKind.Identifier:
                case NodeKind.Constant:
                case NodeKind.ExportFlag:
                case NodeKind.ByValModifier:
                case NodeKind.AnnotationParameterName:
                case NodeKind.AnnotationParameterValue:
                case NodeKind.ParameterDefaultValue:
                    return new TerminalNode(kind, startLexem);
                case NodeKind.BlockEnd:
                    return new LabelNode(startLexem.Location);
                default:
                    var node = MakeNonTerminal(kind, startLexem);
                    if (RootNode == default)
                        RootNode = node;
                    return node;
            }
        }

        private static NonTerminalNode MakeNonTerminal(int kind, in Lexem startLexem)
        {
            switch (kind)
            {
                case NodeKind.Annotation:
                    return new AnnotationNode
                    {
                        Name = startLexem.Content
                    };
                case NodeKind.AnnotationParameter:
                    return new AnnotationParameterNode();
                case NodeKind.VariableDefinition:
                    return new VariableDefinitionNode(startLexem);
                case NodeKind.Method:
                    return new MethodNode();
                case NodeKind.MethodSignature:
                    return new MethodSignatureNode(startLexem);
                case NodeKind.MethodParameter:
                    return new MethodParameterNode();
                case NodeKind.BinaryOperation:
                    return new BinaryOperationNode(startLexem);
                case NodeKind.UnaryOperation:
                    return new UnaryOperationNode(startLexem);
                case NodeKind.WhileLoop:
                    return new WhileLoopNode();
                case NodeKind.Condition:
                    return new ConditionNode();
                default:
                    return new NonTerminalNode(kind, startLexem);
            }
        }

        public virtual void AddChild(IAstNode parent, IAstNode child)
        {
            var parentNonTerm = (NonTerminalNode) parent;
            var childTerm = (BslSyntaxNode) child;
            parentNonTerm.AddChild(childTerm);
        }

        public virtual void HandleParseError(in ParseError error, in Lexem lexem, ILexemGenerator lexer)
        {
            if(ThrowOnError)
                throw new SyntaxErrorException(error.Position, error.Description);
        }

        public virtual void PreprocessorDirective(ILexemGenerator lexer, ref Lexem lastExtractedLexem)
        {
        }
    }
}