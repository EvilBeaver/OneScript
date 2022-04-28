/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace OneScript.Language
{
    public class ListErrorSink : IErrorSink
    {
        private List<CodeError> _errors;

        public IEnumerable<CodeError> Errors => _errors;

        public bool HasErrors => _errors != default && _errors.Count > 0;
        
        public void AddError(CodeError err)
        {
            if (_errors == default)
                _errors = new List<CodeError>();
            
            _errors.Add(err);
        }
    }
}