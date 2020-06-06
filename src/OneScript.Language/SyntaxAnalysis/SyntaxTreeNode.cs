/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace OneScript.Language.SyntaxAnalysis
{
    public abstract class SyntaxTreeNode : ISyntaxNode, IEquatable<SyntaxTreeNode>
    {
        public ISyntaxNode Parent { get; protected set; }
        
        public IEnumerable<ISyntaxNode> ChildNodes { get; protected set; }
        
        public bool Equals(SyntaxTreeNode other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || CheckEquality(other);
        }

        public SyntaxNodeKind Kind => SyntaxNodeKind.Node;
        
        protected abstract bool CheckEquality(SyntaxTreeNode other);
    }
}