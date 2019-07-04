/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language
{
    public class CodePositionInfo
    {
        public CodePositionInfo()
        {
            LineNumber = -1;
            ColumnNumber = -1;
        }

        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string Code { get; set; }
        public string ModuleName { get; set; }

        public const int OUT_OF_TEXT = -1;
    }
}
