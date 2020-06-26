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
        
        public virtual IAstNode CreateNode(NodeKind kind, in Lexem startLexem)
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
                default:
                    var node = MakeNonTerminal(kind, startLexem);
                    if (RootNode == default)
                        RootNode = node;
                    return node;
            }
        }

        private static NonTerminalNode MakeNonTerminal(NodeKind kind, in Lexem startLexem)
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
                case NodeKind.Method:
                    return new MethodNode();
                case NodeKind.MethodSignature:
                    return new MethodSignatureNode();
                case NodeKind.MethodParameter:
                    return new MethodParameterNode();
                default:
                    return new NonTerminalNode(kind, startLexem);
            }
        }

        public virtual void AddChild(IAstNode parent, IAstNode child)
        {
            var parentNonTerm = (NonTerminalNode) parent;
            var childTerm = (AstNodeBase) child;
            parentNonTerm.AddChild(childTerm);
        }

        public virtual void HandleParseError(in ParseError error, in Lexem lexem, ILexemGenerator lexer)
        {
        }

        public virtual void PreprocessorDirective(ILexemGenerator lexer, ref Lexem lastExtractedLexem)
        {
        }
    }
}