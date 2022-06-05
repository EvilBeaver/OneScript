/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language;

namespace OneScript.Compilation
{
    public class CompilerException : ScriptException
    {
        public CompilerException(string message) : base(message)
        {
        }

        public CompilerException(string message, ErrorPositionInfo errorInfo, Exception innerException = null) : base(errorInfo, message, innerException)
        {
        }

        public CompilerException(ErrorPositionInfo errorInfo, Exception innerException) : base(errorInfo, innerException)
        {
        }

        public CompilerException(Exception innerException) : base(innerException)
        {
        }
        
        public static CompilerException FromCodeError(CodeError error)
        {
            var exc = new CompilerException(error.Description);
            if (error.Position != default)
                AppendCodeInfo(exc, error.Position);

            return exc;
        }
        
        public static void AppendCodeInfo(CompilerException exc, ErrorPositionInfo errorPosInfo)
        {
            exc.LineNumber = errorPosInfo.LineNumber;
            exc.ColumnNumber = errorPosInfo.ColumnNumber;
            exc.Code = errorPosInfo.Code;
            exc.ModuleName = errorPosInfo.ModuleName;
        }
    }
}