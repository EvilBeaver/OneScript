/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using OneScript.Sources;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class ModuleNode : AnnotatableNode
    {
        public ModuleNode(SourceCode source, Lexem startLexem) : base(NodeKind.Module)
        {
            Source = source;
            Location = startLexem.Location;
        }
        
        public SourceCode Source { get; }
    }
}