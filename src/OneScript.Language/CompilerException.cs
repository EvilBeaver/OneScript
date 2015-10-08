using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Language
{
    public class CompilerException : ScriptException
    {
        public CompilerException(string message)
            : this(new CodePositionInfo(), message, null)
        {

        }

        public CompilerException(CodePositionInfo codeInfo, string message)
            : this(codeInfo, message, null)
        {

        }

        public CompilerException(CodePositionInfo codeInfo, string message, Exception innerException)
            : base(codeInfo, message, innerException)
        {
        }

        public static CompilerException MethodIsNotDefined(string name)
        {
            return new CompilerException("Метод не определен: " + name);
        }

        public static CompilerException VariableIsNotDefined(string name)
        {
            return new CompilerException("Переменная не определена: " + name);
        }

        public static Exception UnexpectedEndOfText()
        {
            return new CompilerException("Обнаружено логическое завершение текста модуля");
        }

        public static CompilerException UnexpectedOperation()
        {
            return new CompilerException("Неизвестная операция");
        }

        public static CompilerException IdentifierExpected()
        {
            return new CompilerException("Ожидается идентификатор");
        }

        public static CompilerException SemicolonExpected()
        {
            return new CompilerException("Ожидается символ ; (точка с запятой)");
        }

        public static CompilerException LateVarDefinition()
        {
            return new CompilerException("Объявления переменных должны быть расположены в начале модуля, процедуры или функции");
        }

        public static CompilerException LateMethodDefinition()
        {
            return new CompilerException("Определения процедур и функций должны размещаться перед операторами тела модуля");
        }

        public static CompilerException ExpressionSyntax()
        {
            return new CompilerException("Ошибка в выражении");
        }

        public static CompilerException TokenExpected(string tokens)
        {
            return new CompilerException("Ожидается символ: " + tokens);
        }

        public static CompilerException TokenExpected(Token token)
        {
            return new CompilerException("Ожидается символ: " + token.ToString());
        }

        public static CompilerException ExpressionExpected()
        {
            return new CompilerException("Ожидается выражение");
        }

        public static CompilerException LiteralExpected()
        {
            return new CompilerException("Ожидается значение примитивного типа");
        }

        public static CompilerException ExportOnLocalVariable()
        {
            return new CompilerException("Локальная переменная не может быть экспортирована");
        }

        public static CompilerException MisplacedBreakStatement()
        {
            return new CompilerException("Оператор 'Прервать' может использоваться только внутри цикла");
        }

        public static CompilerException MisplacedContinueStatement()
        {
            throw new CompilerException("Оператор 'Продолжить' может использоваться только внутри цикла");
        }

        public static CompilerException MisplacedRaiseException()
        {
            return new CompilerException("Оператор \"ВызватьИсключение\" без параметров может использоваться только в блоке \"Исключение\"");
        }

        public static CompilerException ReturnOutsideOfMethod()
        {
            return new CompilerException("Оператор \"Возврат\" может использоваться только внутри метода");
        }

        public static CompilerException FuncEmptyReturnValue()
        {
            return new CompilerException("Функция должна возвращать значение");
        }
    }
}
