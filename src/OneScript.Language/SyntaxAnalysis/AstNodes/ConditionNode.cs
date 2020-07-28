/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class ConditionNode : BranchingStatementNode
    {
        private int _alternativesStart = 0;
        
        public ConditionNode(Lexem startLexem) : base(NodeKind.Condition, startLexem)
        {
        }
        
        public BslSyntaxNode Expression { get; private set; }
            
        public CodeBatchNode TruePart { get; private set; }

        public IEnumerable<BslSyntaxNode> GetAlternatives()
        {
            if(_alternativesStart == 0)
                return new BslSyntaxNode[0];

            return Children
                .Skip(_alternativesStart)
                .TakeWhile(x=>x.Kind != NodeKind.BlockEnd);
        }

        protected override void OnChildAdded(BslSyntaxNode child)
        {
            switch (child.Kind)
            {
                case NodeKind.CodeBatch when TruePart == default:
                    TruePart = (CodeBatchNode) child;
                    break;
                case NodeKind.CodeBatch:
                case NodeKind.Condition:
                    UpdateAlternatives();
                    break;
                case NodeKind.BlockEnd:
                    base.OnChildAdded(child);
                    break;
                default:
                    Expression = child;
                    break;
            }
        }
        
        private void UpdateAlternatives()
        {
            if (_alternativesStart == 0)
                _alternativesStart = Children.Count-1;
            else
                _alternativesStart++;
        }
    }
}