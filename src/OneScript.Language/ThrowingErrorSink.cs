/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace OneScript.Language
{
    public class ThrowingErrorSink : IErrorSink
    {
        public ThrowingErrorSink()
        {
            ExceptionFactory = err => new SyntaxErrorException(err);
        }
        
        public ThrowingErrorSink(Func<CodeError, ScriptException> exceptionFactory)
        {
            ExceptionFactory = exceptionFactory;
        }

        public IEnumerable<CodeError> Errors => Array.Empty<CodeError>();
            
        public bool HasErrors => false;

        private Func<CodeError, ScriptException> ExceptionFactory { get; set; }
        
        public void AddError(CodeError err)
        {
            throw ExceptionFactory(err);
        }
    }
}