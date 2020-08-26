/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Compiler.ByteCode;

namespace OneScript.Language.Tests
{
    public class TestAstNode : BslSyntaxNode
    {
        private Lazy<IReadOnlyList<TestAstNode>> _childrenLazy;
        
        public BslSyntaxNode RealNode { get; }
        
        public TestAstNode(BslSyntaxNode node)
        {
            RealNode = node;
            Kind = node.Kind;
            if (node is NonTerminalNode nonTerm)
            {
                _childrenLazy = new Lazy<IReadOnlyList<TestAstNode>>(nonTerm.Children.Select(x => new TestAstNode(x)).ToArray());
                Value = nonTerm switch
                {
                    AnnotationNode anno => anno.Name,
                    BinaryOperationNode binary => binary.Operation.ToString(),
                    UnaryOperationNode unary => unary.Operation.ToString(),
                    PreprocessorDirectiveNode preproc => preproc.DirectiveName,
                    _ => Value
                };
            }
            else if(node is TerminalNode term)
            {
                _childrenLazy = new Lazy<IReadOnlyList<TestAstNode>>(new TestAstNode[0]);
                Value = term.Lexem.Content;
            }
        }
        
        public string Value { get; set; }

        public IReadOnlyList<TestAstNode> ChildrenList => _childrenLazy.Value;

        public override IReadOnlyList<BslSyntaxNode> Children => ChildrenList;
        
        public override string ToString()
        {
            return $"{NodeKind.Presentation(Kind)} ({Location.LineNumber},{Location.ColumnNumber})";
        }
    }
}