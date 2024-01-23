/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    /// <summary>
    /// Нода ошибочного синтаксиса
    /// </summary>
    public class ErrorTerminalNode : TerminalNode
    {
        public ErrorTerminalNode() : base(NodeKind.Unknown)
        {
        }

        public ErrorTerminalNode(Lexem lexem) : base(NodeKind.Unknown, lexem)
        {
        }
    }
}