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


        internal static Exception UnexpectedEndOfText()
        {
            return new CompilerException("Обнаружено логическое завершение текста модуля");
        }
    }
}
