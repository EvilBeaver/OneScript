/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Native
{
    public class ExpressionNode : BslSyntaxNode
    {
        public BslSyntaxNode Node { get; set; }
        
        public TypeDescriptor Type { get; set; }

        public override IReadOnlyList<BslSyntaxNode> Children => Node.Children;
    }
}