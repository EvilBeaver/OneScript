/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Runtime.CompilerServices;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    internal static class NonTerminalExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddNode<T>(this NonTerminalNode parent, T child)
            where T : BslSyntaxNode
        {
            parent.AddChild(child);
            return child;
        }
    }
}