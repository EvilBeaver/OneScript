/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language;

namespace ScriptEngine.Compiler.Extensions
{
    public static class ExceptionHelperExtensions
    {
        public static ScriptException AppendCodeInfo(ScriptException exc, ErrorPositionInfo errorPosInfo)
        {
            exc.LineNumber = errorPosInfo.LineNumber;
            exc.ColumnNumber = errorPosInfo.ColumnNumber;
            exc.Code = errorPosInfo.Code;
            exc.ModuleName = errorPosInfo.ModuleName;

            return exc;
        }
    }
}