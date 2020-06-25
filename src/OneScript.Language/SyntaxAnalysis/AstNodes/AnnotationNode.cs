/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace ScriptEngine.Compiler.ByteCode
{
    public class AnnotationNode : NonTerminalNode
    {
        public string Name { get; set; }
    }
}