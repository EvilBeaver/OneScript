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
        public CodeRange(int line, int column)
        {
            LineNumber = line;
            ColumnNumber = column;
        }

        public static CodeRange EmptyRange()
        {
            return new CodeRange(-1,-1);
        }
        
        public int LineNumber { get; }
        
        public int ColumnNumber { get; }
    }
}