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
    public class MethodSignatureNode : NonTerminalNode
    {
        public MethodSignatureNode(Lexem startLexem) 
            : base (NodeKind.MethodSignature, startLexem)
        {
        }

        public bool IsFunction { get; set; }

        public string MethodName { get; set; }
        
        public bool IsExported { get; set; }

        public IEnumerable<MethodParameterNode> GetParameters()
        {
            var paramList = Children.FirstOrDefault(x => x.Kind == NodeKind.MethodParameters);
            if (paramList == default)
                return new MethodParameterNode[0];

            return ((NonTerminalNode) paramList).Children.Cast<MethodParameterNode>();
        }

        protected override void OnChildAdded(BslSyntaxNode child)
        {
            base.OnChildAdded(child);
            switch (child.Kind)
            {
                case NodeKind.Procedure:
                    IsFunction = false;
                    break;
                case NodeKind.Function:
                    IsFunction = true;
                    break;
                case NodeKind.Identifier:
                    MethodName = ((TerminalNode) child).Lexem.Content;
                    break;
                case NodeKind.ExportFlag:
                    IsExported = true;
                    break;
            }
        }
    }
}