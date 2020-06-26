/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class MethodParameterNode : NonTerminalNode
    {
        public MethodParameterNode() : base(NodeKind.MethodParameter)
        {
        }

        public bool IsByValue { get; private set; }
        
        public string Name { get; private set; }
        
        public bool HasDefaultValue { get; private set; }

        public Lexem DefaultValue { get; private set; }

        protected override void OnChildAdded(AstNodeBase child)
        {
            base.OnChildAdded(child);
            switch (child.Kind)
            {
                case NodeKind.ByValModifier:
                    IsByValue = true;
                    break;
                case NodeKind.Identifier:
                    Name = ((TerminalNode) child).Lexem.Content;
                    break;
                case NodeKind.ParameterDefaultValue:
                    DefaultValue = ((TerminalNode) child).Lexem;
                    HasDefaultValue = true;
                    break;
            }
        }
    }
}