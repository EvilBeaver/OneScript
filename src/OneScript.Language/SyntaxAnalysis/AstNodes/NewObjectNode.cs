/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class NewObjectNode : NonTerminalNode
    {
        public NewObjectNode(Lexem startLexem) : base(NodeKind.NewObject, startLexem)
        {
        }

        public bool IsDynamic { get; private set; }
        
        public BslSyntaxNode TypeNameNode { get; private set; }
        
        public BslSyntaxNode ConstructorArguments { get; private set; }

        protected override void OnChildAdded(BslSyntaxNode child)
        {
            if (Children.Count == 1)
            {
                if (child.Kind == NodeKind.CallArgument)
                {
                    IsDynamic = true;
                    TypeNameNode = child.Children[0];
                }
                else
                {
                    IsDynamic = false;
                    TypeNameNode = child;
                }
            }
            else
            {
                ConstructorArguments = child;
            }
        }
    }
}