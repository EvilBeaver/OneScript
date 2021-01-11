/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ListErrorSink : IErrorSink
    {
        private List<ParseError> _errors;

        public IEnumerable<ParseError> Errors => _errors;

        public bool HasErrors => _errors != default && _errors.Count > 0;
        
        public void AddError(ParseError err)
        {
            if (_errors == default)
                _errors = new List<ParseError>();
            
            _errors.Add(err);
        }
    }
}