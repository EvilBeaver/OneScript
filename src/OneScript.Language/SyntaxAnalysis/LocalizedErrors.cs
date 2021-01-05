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
using OneScript.Commons;
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
                Description = Locale.NStr(description)
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

        public static ParseError LiteralExpected() => Create("ru='Ожидается константа';en='Constant expected'");

        public static ParseError NumberExpected() => Create("ru='Ожидается числовая константа';en='Numeric constant expected'");

        public static ParseError UnexpectedEof() =>
            Create("ru='Неожиданный конец модуля';en = 'Unexpected end of text'");

        public static ParseError BreakOutsideOfLoop() =>
            Create("ru='Оператор \"Прервать\" может использоваться только внутри цикла';en='Break operator may be used only within loop'");

        public static ParseError ContinueOutsideLoop() =>
            Create("ru='Оператор \"Продолжить\" может использоваться только внутри цикла';en='Continue operator may be used only within loop'");

        public static ParseError FuncEmptyReturnValue() =>
            Create("ru='Функция должна возвращать значение';en='Function should return a value'");

        public static ParseError ProcReturnsAValue() =>
            Create("ru='Процедуры не могут возвращать значение';en='Procedures cannot return value'");

        public static ParseError ReturnOutsideOfMethod() => Create("ru='Оператор \"Возврат\" может использоваться только внутри метода';"+
                                                                   "en='Return operator may not be used outside procedure or function'");

        public static ParseError MismatchedRaiseException() =>
            Create("ru='Оператор \"ВызватьИсключение\" без параметров может использоваться только в блоке \"Исключение\"';" + 
                   "en='Raise operator may be used without arguments only when handling exception'");
        
        public static ParseError WrongEventName() =>
            Create("ru = 'Ожидается имя события'; en = 'Event name expected'");
        
        public static ParseError WrongHandlerName() =>
            Create("ru = 'Ожидается имя обработчика события'; en = 'Event handler name expected'");
        
        public static ParseError UnexpectedSymbol(char c) =>
            Create($"ru = 'Неизвестный символ {c}'; en = 'Unexpected character {c}'");

        public static ParseError DirectiveNotSupported(string directive) =>
            Create($"ru ='Директива {directive} не разрешена в данном месте'; en = 'Directive {directive} is not supported here'");
        
        public static ParseError EndOfDirectiveExpected(string directive) =>
            Create($"ru ='Ожидается завершение директивы препроцессора #{directive}'; en = 'End of directive #{directive} expected'");

        public static ParseError RegionNameExpected() =>
            Create("ru = 'Ожидается имя области';en = 'Region name expected'");
        
        public static ParseError InvalidRegionName(string name) =>
            Create($"ru = 'Недопустимое имя Области: {name}';en = 'Invalid Region name {name}'");

        public static ParseError DirectiveIsMissing(string directive) =>
            Create(
                $"ru ='Пропущена директива #{directive}'; en = 'Directive #{directive} is missing'");

        public static ParseError LibraryNameExpected() =>
            Create("ru = 'Ожидается имя библиотеки;en='Library name expected'");
    }
}