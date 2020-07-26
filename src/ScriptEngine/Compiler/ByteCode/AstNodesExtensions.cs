/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Runtime.CompilerServices;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace ScriptEngine.Compiler.ByteCode
{
    public static class AstNodesExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetIdentifier(this BslSyntaxNode node)
        {
            return GetIdentifier((TerminalNode) node);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetIdentifier(this TerminalNode node)
        {
            return node.Lexem.Content;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Lexem GetLexem(this TerminalNode node)
        {
            return node.Lexem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TerminalNode AsTerminal(this BslSyntaxNode node)
        {
            return node as TerminalNode;
        }
    }
}