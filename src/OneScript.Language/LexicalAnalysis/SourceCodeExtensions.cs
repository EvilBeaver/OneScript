/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public static class SourceCodeExtensions
    {
        public static ErrorPositionInfo GetErrorPosition(this SourceCodeIterator iterator)
        {
            return new ErrorPositionInfo
            {
                LineNumber = iterator.CurrentLine,
                ColumnNumber = iterator.CurrentColumn,
                Code = iterator.GetCodeLine(iterator.CurrentLine)
            };
        }
        
        public static ErrorPositionInfo GetErrorPosition(this ILexemGenerator lexer)
        {
            return lexer.Iterator.GetErrorPosition();
        }
    }
}