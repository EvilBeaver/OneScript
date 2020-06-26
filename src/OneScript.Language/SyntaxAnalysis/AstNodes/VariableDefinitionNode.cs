/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class VariableDefinitionNode : AnnotatableNode
    {
        public VariableDefinitionNode()
        : base(NodeKind.VariableDefinition)
        {
        }
        
        public string Name { get; set; }

        public bool IsExported { get; private set; }

        protected override void OnChildAdded(AstNodeBase child)
        {
            if (child.Kind == NodeKind.ExportFlag)
                IsExported = true;
            else if (child.Kind == NodeKind.Identifier && child is TerminalNode term)
                Name = term.Lexem.Content;
            else
                base.OnChildAdded(child);
        }
    }
}