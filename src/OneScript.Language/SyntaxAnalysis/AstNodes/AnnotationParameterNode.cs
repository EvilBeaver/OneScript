/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class AnnotationParameterNode : NonTerminalNode
    {
        public AnnotationParameterNode() : base(NodeKind.AnnotationParameter)
        {
        }
        
        protected override void OnChildAdded(BslSyntaxNode child)
        {
            var node = (TerminalNode) child;
            if (child.Kind == NodeKind.AnnotationParameterName)
            {
                Name = node.Lexem.Content;
            }
            if (child.Kind == NodeKind.AnnotationParameterValue)
            {
                Value = node.Lexem;
            }
        }

        public string Name { get; private set; }
        
        public Lexem Value { get; private set; }
    }
}