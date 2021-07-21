/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using OneScript.Language.LexicalAnalysis;
using OneScript.Localization;

namespace OneScript.Language.SyntaxAnalysis
{
    public static class LocalizedErrors
    {
        public static ParseError UnexpectedOperation()
        {
            return Create("Неизвестная операция", "Unknown operation");
        }

        public static ParseError LateVarDefinition()
        {
            return Create("Объявления переменных должны быть расположены в начале модуля, процедуры или функции",
                          "Variable declarations must be placed at beginning of module, procedure, or function");
        }

        public static ParseError SemicolonExpected() 
            => Create("Ожидается символ ; (точка с запятой)", "Expecting \";\"");

        public static ParseError ExpressionExpected() =>
            Create("Ожидается выражение", "Expression expected");

        private static ParseError Create(string ru, string en, [CallerMemberName] string errorId = default)
        {
            return new ParseError
            {
                ErrorId = errorId,
                Description = BilingualString.Localize(ru, en)
            };
        }

        public static ParseError IdentifierExpected() 
            => Create("Ожидается идентификатор", "Identifier expecting");

        public static ParseError ExpressionSyntax()
            => Create("Ошибка в выражении", "Expression syntax error");
        
        public static ParseError TokenExpected(params Token[] expected)
        {
            var names = String.Join("/", expected.Select(x => Enum.GetName(typeof(Token), x)));
            
            return Create($"Ожидается символ: {names}", $"Expecting symbol: {names}");
        }

        public static ParseError ExportedLocalVar(string varName)
        {
            return Create($"Локальная переменная не может быть экспортирована ({varName})",
                    $"Local variable can't be exported ({varName})");
        }

        public static ParseError LiteralExpected() => Create("Ожидается константа", "Constant expected");

        public static ParseError NumberExpected() => Create("Ожидается числовая константа", "Numeric constant expected");

        public static ParseError UnexpectedEof() =>
            Create("Неожиданный конец модуля", "Unexpected end of text");

        public static ParseError BreakOutsideOfLoop() =>
            Create("Оператор \"Прервать\" может использоваться только внутри цикла", "Break operator may be used only within loop");

        public static ParseError ContinueOutsideLoop() =>
            Create("Оператор \"Продолжить\" может использоваться только внутри цикла", "Continue operator may be used only within loop");

        public static ParseError FuncEmptyReturnValue() =>
            Create("Функция должна возвращать значение", "Function should return a value");

        public static ParseError ProcReturnsAValue() =>
            Create("Процедуры не могут возвращать значение", "Procedures cannot return value");

        public static ParseError ReturnOutsideOfMethod() => Create("Оператор \"Возврат\" может использоваться только внутри метода",
            "Return operator may not be used outside procedure or function");

        public static ParseError MismatchedRaiseException() =>
            Create("Оператор \"ВызватьИсключение\" без параметров может использоваться только в блоке \"Исключение\"",
                   "Raise operator may be used without arguments only when handling exception");
        
        public static ParseError WrongEventName() =>
            Create("Ожидается имя события", "Event name expected");
        
        public static ParseError WrongHandlerName() =>
            Create("Ожидается имя обработчика события", "Event handler name expected");
        
        public static ParseError UnexpectedSymbol(char c) =>
            Create($"Неизвестный символ {c}", $"Unexpected character {c}");

        public static ParseError DirectiveNotSupported(string directive) =>
            Create($"Директива {directive} не разрешена в данном месте", $"Directive {directive} is not supported here");
        
        public static ParseError EndOfDirectiveExpected(string directive) =>
            Create($"Ожидается завершение директивы препроцессора #{directive}",
                $"End of directive #{directive} expected");
        
        public static ParseError DirectiveExpected(string directive) =>
            Create($"Ожидается директива препроцессора #{directive}", $"Preprocessor directive #{directive} expected");

        public static ParseError RegionNameExpected() =>
            Create("Ожидается имя области", "Region name expected");
        
        public static ParseError InvalidRegionName(string name) =>
            Create($"Недопустимое имя Области: {name}", $"Invalid Region name {name}");

        public static ParseError DirectiveIsMissing(string directive) =>
            Create($"Пропущена директива #{directive}", $"Directive #{directive} is missing");

        public static ParseError LibraryNameExpected() =>
            Create("Ожидается имя библиотеки", "Library name expected");
        
        public static ParseError PreprocessorDefinitionExpected() =>
            Create("Ожидается объявление препроцессора", "Preprocessor definition expected");
        
        public static ParseError UseBuiltInFunctionAsProcedure() =>
            Create("Использование встроенной функции, как процедуры", "Using build-in function as procedure");
    }
}