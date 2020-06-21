/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using OneScript.Language.LexicalAnalysis;

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
            => Create("ru='Ожидается символ ; (точка с запятой)';en='Expecting \";\"'");

        public static ParseError ExpressionExpected() =>
            Create("ru='Ожидается выражение';en='Expression expected'");
        
        private static ParseError Create(string description, [CallerMemberName] string errorId = default)
        {
            return new ParseError
            {
                ErrorId = errorId,
                Description = description
            };  
        }

        public static ParseError IdentifierExpected() 
            => Create("ru='Ожидается идентификатор';en='Identifier expecting'");

        public static ParseError ExpressionSyntax()
            => Create("ru='Ошибка в выражении';en='Expression syntax error'");
        
        public static ParseError TokenExpected(params Token[] expected)
        {
            var names = String.Join("/", expected.Select(x => Enum.GetName(typeof(Token), x)));
            
            return Create($"ru='Ожидается символ: {names}';en='Expecting symbol: {names}'");
        }
        
        public static ParseError ExportedLocalVar(string varName)
        {
            return Create($"ru = 'Локальная переменная не может быть экспортирована ({varName})';" +
                          "en = 'Local variable can't be exported ({varName})'");
        }

        public static ParseError LiteralExpected()
        {
            return Create("ru='Ожидается константа';en='Constant expected'");
        }

        public static ParseError NumberExpected()
        {
            return Create("ru='Ожидается числовая константа';en='Numeric constant expected'");
        }
    }
}