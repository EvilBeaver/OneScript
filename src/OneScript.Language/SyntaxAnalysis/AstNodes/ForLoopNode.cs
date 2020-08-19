/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class ForLoopNode : BranchingStatementNode
    {
        public ForLoopNode(Lexem startLexem) : base(NodeKind.ForLoop, startLexem)
        {
        }

        public BslSyntaxNode InitializationClause { get; set; }

        public BslSyntaxNode UpperLimitExpression { get; set; }
        
        public CodeBatchNode LoopBody { get; set; }
        
        protected override void OnChildAdded(BslSyntaxNode child)
        {
            switch (child.Kind)
            {
                case NodeKind.ForInitializer:
                    InitializationClause = child;
                    break;
                case NodeKind.ForLimit:
                    UpperLimitExpression = child.Children[0];
                    break;
                case NodeKind.CodeBatch:
                    if (UpperLimitExpression == default)
                    {
                        InitializationClause = new NonTerminalNode(NodeKind.Unknown);
                        UpperLimitExpression = new NonTerminalNode(NodeKind.Unknown);
                    }

                    LoopBody = (CodeBatchNode)child;
                    break;
                case NodeKind.BlockEnd:
                    base.OnChildAdded(child);
                    break;
            }
        }
    }
}