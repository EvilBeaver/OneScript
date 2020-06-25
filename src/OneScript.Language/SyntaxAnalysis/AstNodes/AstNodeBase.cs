/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public abstract class AstNodeBase : IAstNode
    {
        public NodeKind Kind { get; protected set; }
        
        public IAstNode Parent { get; internal set; }
    }
}