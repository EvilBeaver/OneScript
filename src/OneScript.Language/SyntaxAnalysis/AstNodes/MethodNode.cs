/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class MethodNode : AnnotatableNode
    {
        private int _variableStart = -1;
        private int _variableCount;
        
        public MethodNode() : base(NodeKind.Method)
        {
        }

        public MethodSignatureNode Signature { get; private set; }
        
        public BslSyntaxNode MethodBody { get; private set; }

        public IReadOnlyList<VariableDefinitionNode> VariableDefinitions()
        {
            if (_variableCount == 0)
                return new VariableDefinitionNode[0];
            
            return Children
                .Skip(_variableStart)
                .Take(_variableCount)
                .Cast<VariableDefinitionNode>()
                .ToList()
                .AsReadOnly();
        }

        protected override void OnChildAdded(BslSyntaxNode child)
        {
            switch (child.Kind)
            {
                case NodeKind.MethodSignature:
                    Signature = (MethodSignatureNode) child;
                    break;
                case NodeKind.CodeBatch:
                    MethodBody = child;
                    break;
                case NodeKind.VariableDefinition:
                {
                    if (_variableStart == -1)
                        _variableStart = Children.Count;
                    _variableCount++;
                    break;
                }
                default:
                    base.OnChildAdded(child);
                    break;
            }
        }
    }
}