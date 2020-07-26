/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class LabelNode : BslSyntaxNode
    {
        public CodeRange Location { get; }

        public LabelNode(CodeRange location)
        {
            Location = location;
        }
        
        public override IReadOnlyList<BslSyntaxNode> Children => new BslSyntaxNode[0];
    }
}