/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using ScriptEngine.Environment;

namespace ScriptEngine.Compiler
{
    public static class CodeNavigationExtensions
    {
        public static ErrorPositionInfo ToCodePosition(this CodeRange range, ModuleInformation moduleInfo)
        {
            return new ErrorPositionInfo()
            {
                Code = moduleInfo.GetCodeLine(range.LineNumber),
                LineNumber = range.LineNumber,
                ColumnNumber = range.ColumnNumber,
                ModuleName = moduleInfo.ModuleName
            };
        }

        public static string GetCodeLine(this ModuleInformation info, int line)
        {
            return info.CodeIndexer.GetCodeLine(line);
        }
    }
}