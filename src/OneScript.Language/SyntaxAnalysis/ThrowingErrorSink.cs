/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ThrowingErrorSink : IErrorSink
    {
        public IEnumerable<ParseError> Errors { get; }
            
        public bool HasErrors => false;
            
        public void AddError(ParseError err)
        {
            throw new SyntaxErrorException(err);
        }
    }
}