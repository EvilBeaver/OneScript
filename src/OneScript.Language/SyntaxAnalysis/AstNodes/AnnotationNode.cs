/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class AnnotationNode : NonTerminalNode
    {
        public AnnotationNode(NodeKind kind, Lexem startLexem) : base(kind, startLexem)
        {
            if(Kind != NodeKind.Annotation && Kind != NodeKind.Import)
                throw new ArgumentException(nameof(kind));
        }
        
        public string Name { get; set; }
    }
}