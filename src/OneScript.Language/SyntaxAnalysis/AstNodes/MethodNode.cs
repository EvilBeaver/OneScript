/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class MethodNode : AnnotatableNode
    {
        public MethodNode() : base(NodeKind.Method)
        {
        }

        public MethodSignatureNode Signature { get; private set; }

        protected override void OnChildAdded(AstNodeBase child)
        {
            if (child.Kind == NodeKind.MethodSignature)
            {
                Signature = (MethodSignatureNode) child;
            }
            else
            {
                base.OnChildAdded(child);
            }
        }
    }
}