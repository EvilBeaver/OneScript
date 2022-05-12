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
        public static CodeError UnexpectedOperation()
        {
            return Create("Неизвестная операция", "Unknown operation");
        }

        public static CodeError LateVarDefinition()
        {
            return Create("Объявления переменных должны быть расположены в начале модуля, процедуры или функции",
                          "Variable declarations must be placed at beginning of module, procedure, or function");
        }

        public static CodeError SemicolonExpected() 
            => Create("Ожидается символ ; (точка с запятой)", "Expecting \";\"");

        public static CodeError ExpressionExpected() =>
            Create("Ожидается выражение", "Expression expected");

        private static CodeError Create(string ru, string en, [CallerMemberName] string errorId = default)
        {
            return new CodeError
            {
                ErrorId = errorId,
                Description = BilingualString.Localize(ru, en)
            };
        }

        public static CodeError IdentifierExpected() 
            => Create("Ожидается идентификатор", "Identifier expecting");

        public static CodeError ExpressionSyntax()
            => Create("Ошибка в выражении", "Expression syntax error");
        
        public static CodeError TokenExpected(params Token[] expected)
        {
            var names = String.Join("/", expected.Select(x => Enum.GetName(typeof(Token), x)));
            
            return Create($"Ожидается символ: {names}", $"Expecting symbol: {names}");
        }

        public static CodeError ExportedLocalVar(string varName)
        {
            return Create($"Локальная переменная не может быть экспортирована ({varName})",
                    $"Local variable can't be exported ({varName})");
        }

        public static CodeError LiteralExpected() => Create("Ожидается константа", "Constant expected");

        public static CodeError NumberExpected() => Create("Ожидается числовая константа", "Numeric constant expected");

        public static CodeError UnexpectedEof() =>
            Create("Неожиданный конец модуля", "Unexpected end of text");

        public static CodeError BreakOutsideOfLoop() =>
            Create("Оператор \"Прервать\" может использоваться только внутри цикла", "Break operator may be used only within loop");

        public static CodeError ContinueOutsideLoop() =>
            Create("Оператор \"Продолжить\" может использоваться только внутри цикла", "Continue operator may be used only within loop");

        public static CodeError FuncEmptyReturnValue() =>
            Create("Функция должна возвращать значение", "Function should return a value");

        public static CodeError ProcReturnsAValue() =>
            Create("Процедуры не могут возвращать значение", "Procedures cannot return value");

        public static CodeError ReturnOutsideOfMethod() => Create("Оператор \"Возврат\" может использоваться только внутри метода",
            "Return operator may not be used outside procedure or function");

        public static CodeError MismatchedRaiseException() =>
            Create("Оператор \"ВызватьИсключение\" без параметров может использоваться только в блоке \"Исключение\"",
                   "Raise operator may be used without arguments only when handling exception");
        
        public static CodeError WrongEventName() =>
            Create("Ожидается имя события", "Event name expected");
        
        public static CodeError WrongHandlerName() =>
            Create("Ожидается имя обработчика события", "Event handler name expected");
        
        public static CodeError UnexpectedSymbol(char c) =>
            Create($"Неизвестный символ {c}", $"Unexpected character {c}");

        public static CodeError DirectiveNotSupported(string directive) =>
            Create($"Директива {directive} не разрешена в данном месте", $"Directive {directive} is not supported here");
        
        public static CodeError EndOfDirectiveExpected(string directive) =>
            Create($"Ожидается завершение директивы препроцессора #{directive}",
                $"End of directive #{directive} expected");
        
        public static CodeError DirectiveExpected(string directive) =>
            Create($"Ожидается директива препроцессора #{directive}", $"Preprocessor directive #{directive} expected");

        public static CodeError RegionNameExpected() =>
            Create("Ожидается имя области", "Region name expected");
        
        public static CodeError InvalidRegionName(string name) =>
            Create($"Недопустимое имя Области: {name}", $"Invalid Region name {name}");

        public static CodeError DirectiveIsMissing(string directive) =>
            Create($"Пропущена директива #{directive}", $"Directive #{directive} is missing");

        public static CodeError LibraryNameExpected() =>
            Create("Ожидается имя библиотеки", "Library name expected");
        
        public static CodeError PreprocessorDefinitionExpected() =>
            Create("Ожидается объявление препроцессора", "Preprocessor definition expected");
        
        public static CodeError UseBuiltInFunctionAsProcedure() =>
            Create("Использование встроенной функции, как процедуры", "Using build-in function as procedure");
    }
}