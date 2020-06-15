/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Dynamic;
using System.Runtime.CompilerServices;

namespace OneScript.Language.SyntaxAnalysis
{
    internal static class LocalizedErrors
    {
        public static ParseError UnexpectedOperation()
        {
            return Create("ru='Неизвестная операция';en='Unknown operation'");
        }

        public static ParseError LateVarDefinition()
        {
            return Create("ru='Объявления переменных должны быть расположены в начале модуля, процедуры или функции';"
                          + "en='Variable declarations must be placed at beginning of module, procedure, or function'");
        }

        public static ParseError SemicolonExpected()
        {
            return Create("ru='Ожидается символ ; (точка с запятой)';en='Expecting \";\"'");
        }

        private static ParseError Create(string description, [CallerMemberName] string errorId = default)
        {
            return new ParseError
            {
                ErrorId = errorId,
                Description = description
            };  
        }

        public static ParseError IdentifierExpected()
        {
            return Create("ru='Ожидается идентификатор';en='Identifier expecting'");
        }

        public static ParseError ExportedLocalVar(string varName)
        {
            return Create($"ru = 'Локальная переменная не может быть экспортирована ({varName})';" +
                          "en = 'Local variable can't be exported ({varName})'");
        }
    }
}