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
    public class TestAstNode : AstNodeBase
    {
        private Lazy<IReadOnlyList<TestAstNode>> _childrenLazy;
        
        public AstNodeBase RealNode { get; }
        
        public TestAstNode(AstNodeBase node)
        {
            RealNode = node;
            Kind = node.Kind;
            if (node is NonTerminalNode nonTerm)
            {
                _childrenLazy = new Lazy<IReadOnlyList<TestAstNode>>(nonTerm.Children.Select(x => new TestAstNode(x)).ToArray());
                if (nonTerm is AnnotationNode anno)
                {
                    Value = anno.Name;
                }

                if (nonTerm is BinaryOperationNode binary)
                {
                    Value = binary.Operation.Content;
                }
                
                if (nonTerm is UnaryOperationNode unary)
                {
                    Value = unary.Operation.Content;
                }
            }
            else
            {
                _childrenLazy = new Lazy<IReadOnlyList<TestAstNode>>(new TestAstNode[0]);
                Value = ((TerminalNode) node).Lexem.Content;
            }
        }
        
        public string Value { get; set; }

        public IReadOnlyList<TestAstNode> ChildrenList => _childrenLazy.Value;
    }
}