/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class VariableDefinitionNode : AnnotatableNode
    {
        public VariableDefinitionNode(Lexem startLexem)
        : base(NodeKind.VariableDefinition)
        {
            Location = startLexem.Location;
        }
        
        public string Name { get; set; }

        public bool IsExported { get; private set; }

        protected override void OnChildAdded(BslSyntaxNode child)
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