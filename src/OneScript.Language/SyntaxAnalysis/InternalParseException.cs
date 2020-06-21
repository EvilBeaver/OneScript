/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Language.SyntaxAnalysis
{
    internal class InternalParseException : Exception
    {
        public ParseError Error { get; }

        public InternalParseException(ParseError error)
        {
            Error = error;
        }

        public override string ToString()
        {
            return Error.Description + base.ToString();
        }
    }
}