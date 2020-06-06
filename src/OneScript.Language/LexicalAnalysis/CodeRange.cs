/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public readonly struct CodeRange
    {
        public CodeRange(int start, int len)
        {
            Start = start;
            Length = len;
            LineNumber = -1;
            ColumnNumber = -1;
        }
        
        public CodeRange(int start, int len, int line, int column)
        {
            Start = start;
            Length = len;
            LineNumber = line;
            ColumnNumber = column;
        }

        public static CodeRange EmptyRange()
        {
            return new CodeRange(-1,-1);
        }
        
        public int Start { get; }
        
        public int LineNumber { get; }
        
        public int ColumnNumber { get; }

        public int Length { get; }
    }
}