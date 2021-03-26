/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using ScriptEngine.Compiler;

namespace OneScript.StandardLibrary.Native
{
    public static class ErrorSinkExtensions
    {
        public static void AddError(this IErrorSink sink, CompilerException exception)
        {
            sink.AddError(new ParseError
            {
                Description = exception.Message,
                Position = exception.GetPosition(),
                ErrorId = exception.GetType().Name
            });
        }
    }
}