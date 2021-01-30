/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.Commons;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;

namespace ScriptEngine.Compiler
{
    public class CompilerException : ScriptException
    {
        public CompilerException(string msg)
            : base(new ErrorPositionInfo(), msg)
        {

        }

        public static CompilerException FromParseError(ParseError error)
        {
            var exc = new CompilerException(Locale.NStr(error.Description));
            if (error.Position != default)
                AppendCodeInfo(exc, error.Position);

            return exc;
        }
        
        public static CompilerException AppendCodeInfo(CompilerException exc, ErrorPositionInfo errorPosInfo)
        {
            exc.LineNumber = errorPosInfo.LineNumber;
            exc.ColumnNumber = errorPosInfo.ColumnNumber;
            exc.Code = errorPosInfo.Code;
            exc.ModuleName = errorPosInfo.ModuleName;
            
            return exc;
        }

        public static CompilerException UnexpectedOperation()
        {
            return new CompilerException(Locale.NStr("ru='Неизвестная операция';en='Unknown operation'"));
        }

        public static CompilerException IdentifierExpected()
        {
            return new CompilerException(Locale.NStr("ru='Ожидается идентификатор';en='Identifier expecting'"));
        }

        public static CompilerException SemicolonExpected()
        {
            return new CompilerException(Locale.NStr("ru='Ожидается символ ; (точка с запятой)';en='Expecting \";\"'"));
        }

        public static CompilerException LateVarDefinition()
        {
            return new CompilerException(Locale.NStr("ru='Объявления переменных должны быть расположены в начале модуля, процедуры или функции';"
                                                    + "en='Variable declarations must be placed at beginning of module, procedure, or function'"));
        }

        public static CompilerException TokenExpected(params Token[] expected)
        {
            var names = expected.Select(x => Enum.GetName(typeof(Token), x));
            return new CompilerException(Locale.NStr("ru='Ожидается символ: ';en='Expecting symbol: '") + String.Join("/", names));
        }

        public static CompilerException TokenExpected(string tokens)
        {
            return new CompilerException(Locale.NStr("ru='Ожидается символ: ';en='Expecting symbol: '") + tokens);
        }

        public static CompilerException ExpressionSyntax()
        {
            return new CompilerException(Locale.NStr("ru='Ошибка в выражении';en='Expression syntax error'"));
        }

        public static CompilerException UseProcAsFunction()
        {
            return new CompilerException(Locale.NStr("ru='Использование процедуры, как функции';en='Procedure called as function'"));            
        }

        public static CompilerException TooFewArgumentsPassed()
        {
            return new CompilerException(Locale.NStr("ru='Недостаточно фактических параметров';en='Not enough actual parameters'"));
        }

        public static CompilerException TooManyArgumentsPassed()
        {
            return new CompilerException(Locale.NStr("ru='Слишком много фактических параметров'; en='Too many actual parameters'"));
        }

        public static CompilerException InternalCompilerError(string reason)
        {
            return new CompilerException(Locale.NStr("ru='Внутренняя ошибка компилятора:';en='Internal compiler error:'") + reason);
        }

        public static CompilerException UnexpectedEndOfText()
        {
            return new CompilerException(Locale.NStr("ru='Обнаружено логическое завершение текста модуля';en='Logical end of module source text encountered'"));
        }

        public static CompilerException BreakOutsideOfLoop()
        {
            return new CompilerException(Locale.NStr("ru='Оператор \"Прервать\" может использоваться только внутри цикла';en='Break operator may be used only within loop'"));
        }

        public static CompilerException ContinueOutsideOfLoop()
        {
            return new CompilerException(Locale.NStr("ru='Оператор \"Продолжить\" может использоваться только внутри цикла';en='Continue operator may be used only within loop'"));
        }

        public static CompilerException ReturnOutsideOfMethod()
        {
            return new CompilerException(Locale.NStr("ru='Оператор \"Возврат\" может использоваться только внутри метода';en='Return operator may not be used outside procedure or function'"));
        }

        public static CompilerException ProcReturnsAValue()
        {
            return new CompilerException(Locale.NStr("ru='Процедуры не могут возвращать значение';en='Procedures cannot return value'"));
        }

        public static CompilerException FuncEmptyReturnValue()
        {
            return new CompilerException(Locale.NStr("ru='Функция должна возвращать значение';en='Function should return a value'"));
        }

        public static CompilerException MismatchedRaiseException()
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
            return new CompilerException(Locale.NStr($"ru='Неизвестная директива: {name} ({arg})';en='Unknown directive: {name} ({arg})'"));
        }

    }

    public class ExtraClosedParenthesis : CompilerException
    {
        internal ExtraClosedParenthesis(ErrorPositionInfo errorPosInfo) : base(Locale.NStr("ru='Ожидается символ: (';en='Expecting symbol: ('"))
        {
            this.LineNumber = errorPosInfo.LineNumber;
            this.Code = errorPosInfo.Code;
        }
    }

}
