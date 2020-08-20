/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class TryExceptNode : BranchingStatementNode
    {
        public TryExceptNode(Lexem startLexem) : base(NodeKind.TryExcept, startLexem)
        {
        }

        public CodeBatchNode TryBlock { get; private set; }

        public CodeBatchNode ExceptBlock { get; private set; }

        protected override void OnChildAdded(BslSyntaxNode child)
        {
            switch (Children.Count)
            {
                case 1:
                    TryBlock = (CodeBatchNode) child;
                    break;
                case 2:
                    ExceptBlock = (CodeBatchNode) child;
                    break;
                default:
                    base.OnChildAdded(child);
                    break;
            }
        }
    }
}