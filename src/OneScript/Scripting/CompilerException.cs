using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class CompilerException : ScriptException
    {
        internal CompilerException(string message)
            : this(new CodePositionInfo(), message, null)
        {

        }

        internal CompilerException(CodePositionInfo codeInfo, string message)
            : this(codeInfo, message, null)
        {

        }

        internal CompilerException(CodePositionInfo codeInfo, string message, Exception innerException)
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

        public static CompilerException ExpressionSyntax()
        {
            return new CompilerException("Ошибка в выражении");
        }
    }
}
