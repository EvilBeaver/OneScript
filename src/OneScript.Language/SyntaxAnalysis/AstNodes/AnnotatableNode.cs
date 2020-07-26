/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using ScriptEngine.Compiler.ByteCode;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class AnnotatableNode : NonTerminalNode
    {
        protected AnnotatableNode()
        {
        }
        
        public AnnotatableNode(int kind) : base(kind)
        {
        }
        
        private List<AnnotationNode> AnnotationsList { get; } = new List<AnnotationNode>();

        public IEnumerable<AnnotationNode> Annotations => AnnotationsList;

        protected override void OnChildAdded(BslSyntaxNode child)
        {
            if(child is AnnotationNode anno)
                AnnotationsList.Add(anno);
        }
    }
}