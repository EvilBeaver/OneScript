/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Text;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public static class ParserExtensions
    {
        public static string Describe(this IEnumerable<ParseError> errors)
        {
            var builder = new StringBuilder(128);
            foreach (var parseError in errors)
            {
                builder.AppendLine($"{parseError.ErrorId} / {parseError.Position.LineNumber}");
            }

            return builder.ToString();
        }

        public static bool IsEmpty(this CodeRange range)
        {
            return range.LineNumber == -1;
        }
    }
}