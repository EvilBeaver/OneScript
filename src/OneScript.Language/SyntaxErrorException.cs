/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.SyntaxAnalysis;

namespace OneScript.Language
{
    public class SyntaxErrorException : ScriptException
    {
        public SyntaxErrorException(ErrorPositionInfo codeInfo, string message):base(codeInfo, message)
        {

        }

        internal SyntaxErrorException(CodeError error) : base(error.Position, error.Description)
        {
            
        }
        
        internal SyntaxErrorException(ErrorPositionInfo codeInfo, CodeError error) : base(codeInfo, error.Description)
        {
            
        }
    }
}