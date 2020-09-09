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
    public class MethodNode : AnnotatableNode
    {
        public MethodNode() : base(NodeKind.Method)
        {
        }

        public MethodSignatureNode Signature { get; private set; }
        
        public BslSyntaxNode MethodBody { get; private set; }

        public BslSyntaxNode VariableSection { get; private set; }
        
        public CodeRange EndLocation { get; private set; }
        
        public IReadOnlyList<VariableDefinitionNode> VariableDefinitions()
        {
            if (VariableSection == default)
                return new VariableDefinitionNode[0];
            
            return VariableSection.Children
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
                case NodeKind.VariablesSection:
                    VariableSection = child;
                    break;
                case NodeKind.BlockEnd:
                    EndLocation = child.Location;
                    break;
                default:
                    base.OnChildAdded(child);
                    break;
            }
        }
    }
}