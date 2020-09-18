/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language
{
    public class SyntaxErrorException : ScriptException
    {
        internal SyntaxErrorException(ErrorPositionInfo codeInfo, string message):base(codeInfo, message)
        {

        }
    }
}