/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace OneScript.Language.SyntaxAnalysis
{
    public interface IErrorSink
    {
        IEnumerable<ParseError> Errors { get; }
        bool HasErrors { get; }
        void AddError(ParseError err);
    }
}