/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;

namespace ScriptEngine.Compiler
{
    public class CompilerException : ScriptException
    {
        public CompilerException(string msg)
            : base(new CodePositionInfo(), msg)
        {

        }

        internal static CompilerException AppendCodeInfo(CompilerException exc, CodePositionInfo codePosInfo)
        {
            exc.LineNumber = codePosInfo.LineNumber;
            exc.ColumnNumber = codePosInfo.ColumnNumber;
            exc.Code = codePosInfo.Code;
            exc.ModuleName = codePosInfo.ModuleName;
            
            return exc;
        }

        internal static CompilerException UnexpectedOperation()
        {
            return new CompilerException(Locale.NStr("ru='Неизвестная операция';en='Unknown operation'"));
        }

        internal static CompilerException IdentifierExpected()
        {
            return new CompilerException(Locale.NStr("ru='Ожидается идентификатор';en='Identifier expecting'"));
        }

        internal static CompilerException SemicolonExpected()
        {
            return new CompilerException(Locale.NStr("ru='Ожидается символ ; (точка с запятой)';en='Expecting \";\"'"));
        }

        internal static CompilerException LateVarDefinition()
        {
            return new CompilerException(Locale.NStr("ru='Объявления переменных должны быть расположены в начале модуля, процедуры или функции';"
                                                    + "en='Variable declarations must be placed at beginning of module, procedure, or function'"));
        }

        internal static CompilerException TokenExpected(params Token[] expected)
        {
            var names = expected.Select(x => Enum.GetName(typeof(Token), x));
            return new CompilerException(Locale.NStr("ru='Ожидается символ: ';en='Expecting symbol: '") + String.Join("/", names));
        }

        internal static CompilerException TokenExpected(string tokens)
        {
            return new CompilerException(Locale.NStr("ru='Ожидается символ: ';en='Expecting symbol: '") + tokens);
        }

        internal static CompilerException ExpressionSyntax()
        {
            return new CompilerException(Locale.NStr("ru='Ошибка в выражении';en='Expression syntax error'"));
        }

        internal static CompilerException UseProcAsFunction()
        {
            return new CompilerException(Locale.NStr("ru='Использование процедуры, как функции';en='Procedure called as function'"));            
        }

        internal static CompilerException TooFewArgumentsPassed()
        {
            return new CompilerException(Locale.NStr("ru='Недостаточно фактических параметров';en='Not enough actual parameters'"));
        }

        internal static CompilerException TooManyArgumentsPassed()
        {
            return new CompilerException(Locale.NStr("ru='Слишком много фактических параметров'; en='Too many actual parameters'"));
        }

        internal static CompilerException InternalCompilerError(string reason)
        {
            return new CompilerException(Locale.NStr("ru='Внутренняя ошибка компилятора:';en='Internal compiler error:'") + reason);
        }

        internal static CompilerException UnexpectedEndOfText()
        {
            return new CompilerException(Locale.NStr("ru='Обнаружено логическое завершение текста модуля';en='Logical end of module source text encountered'"));
        }

        internal static CompilerException BreakOutsideOfLoop()
        {
            return new CompilerException(Locale.NStr("ru='Оператор \"Прервать\" может использоваться только внутри цикла';en='Break operator may be used only within loop'"));
        }

        internal static CompilerException ContinueOutsideOfLoop()
        {
            return new CompilerException(Locale.NStr("ru='Оператор \"Продолжить\" может использоваться только внутри цикла';en='Continue operator may be used only within loop'"));
        }

        internal static CompilerException ReturnOutsideOfMethod()
        {
            return new CompilerException(Locale.NStr("ru='Оператор \"Возврат\" может использоваться только внутри метода';en='Return operator may not be used outside procedure or function'"));
        }

        internal static CompilerException ProcReturnsAValue()
        {
            return new CompilerException(Locale.NStr("ru='Процедуры не могут возвращать значение';en='Procedures cannot return value'"));
        }

        internal static CompilerException FuncEmptyReturnValue()
        {
            return new CompilerException(Locale.NStr("ru='Функция должна возвращать значение';en='Function should return a value'"));
        }

        internal static CompilerException MismatchedRaiseException()
        {
            return new CompilerException(Locale.NStr("ru='Оператор \"ВызватьИсключение\" без параметров может использоваться только в блоке \"Исключение\"';en='Raise operator may be used without arguments only when handling exception'"));
        }

        public static CompilerException ExpressionExpected()
        {
            return new CompilerException(Locale.NStr("ru='Ожидается выражение';en='Expression expected'"));
        }

        public static CompilerException LiteralExpected()
        {
            return new CompilerException(Locale.NStr("ru='Ожидается константа';en='Constant expected'"));
        }

        public static CompilerException NumberExpected()
        {
            return new CompilerException(Locale.NStr("ru='Ожидается числовая константа';en='Numeric constant expected'"));
        }

        public static CompilerException IllegalDirective(string name)
        {
            return new CompilerException(Locale.NStr("ru='Недопустимая директива:';en='Illegal directive'")+name);
        }

        public static CompilerException UnknownDirective(string name, string arg)
        {
            return new CompilerException(Locale.NStr("ru='Неизвестная директива:';en='Unknown directive'") + $"{name} ({arg})");
        }

    }

    public class ExtraClosedParenthesis : CompilerException
    {
        internal ExtraClosedParenthesis(CodePositionInfo codePosInfo) : base(Locale.NStr("ru='Ожидается символ: (';en='Expecting symbol: ('"))
        {
            this.LineNumber = codePosInfo.LineNumber;
            this.Code = codePosInfo.Code;
        }
    }

}
