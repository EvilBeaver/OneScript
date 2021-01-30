/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Compiler.Legacy.LexicalAnalysis
{
    public static class SourceCodeExtensions
    {
        public static ErrorPositionInfo GetErrorPosition(this ILexemGenerator lexer)
        {
            return lexer.Iterator.GetErrorPosition();
        }

        public static ErrorPositionInfo GetErrorPosition(this ILexer lexer)
        {
            return lexer.Iterator.GetErrorPosition();
        }
    }
}