using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Compiler
{
    public class CompilerException : ScriptException
    {
        internal CompilerException(string msg)
            : base(new CodePositionInfo(), msg)
        {

        }

        internal static CompilerException AppendCodeInfo(CompilerException exc, int line, string codeString)
        {
            exc.LineNumber = line;
            exc.Code = codeString;
            return exc;
        }

        internal static CompilerException AppendCodeInfo(CompilerException exc, CodePositionInfo codePosInfo)
        {
            AppendCodeInfo(exc, codePosInfo.LineNumber, codePosInfo.Code);
            return exc;
        }

        internal static CompilerException UnexpectedOperation()
        {
            return new CompilerException("Unexpected operation");
        }

        internal static CompilerException IdentifierExpected()
        {
            return new CompilerException("Identifier expected");
        }

        internal static CompilerException SemicolonExpected()
        {
            return new CompilerException("Semicolon expected");
        }

        internal static CompilerException LateVarDefinition()
        {
            return new CompilerException("Variable declarations must preceed methods and operators");
        }

        internal static CompilerException TokenExpected(params Token[] expected)
        {
            var names = expected.Select(x => Enum.GetName(typeof(Token), x));
            return new CompilerException("Token expected: " + String.Join("/", names));
        }

        internal static CompilerException TokenExpected(string tokens)
        {
            return new CompilerException("Token expected: " + tokens);
        }

        internal static CompilerException ExpressionSyntax()
        {
            return new CompilerException("Expression syntax");
        }

        internal static CompilerException UseProcAsFunction()
        {
            return new CompilerException("Using procedure as function");
        }

        internal static CompilerException TooLittleArgumentsPassed()
        {
            return new CompilerException("Too litte actual parameters have been passed");
        }

        internal static CompilerException TooManyArgumentsPassed()
        {
            return new CompilerException("Too many actual parameters have been passed");
        }

        internal static CompilerException ArgHasNoDefaultValue(int argNum)
        {
            return new CompilerException(string.Format("Argument {0} has no default value", argNum));
        }

        internal static CompilerException InternalCompilerError(string reason)
        {
            return new CompilerException("Internal comiler error:" + reason);
        }

        internal static CompilerException UnexpectedEndOfText()
        {
            return new CompilerException("Unexpected end of text");
        }

        internal static CompilerException BreakOutsideOfLoop()
        {
            return new CompilerException("Break statement outside of loop");
        }

        internal static CompilerException ContinueOutsideOfLoop()
        {
            return new CompilerException("Continue statement outside of loop");
        }

        internal static CompilerException ReturnOutsideOfMethod()
        {
            return new CompilerException("Return statement outside of method");
        }

        internal static CompilerException ProcReturnsAValue()
        {
            return new CompilerException("Procedures cannot return a value");
        }

        internal static CompilerException FuncEmptyReturnValue()
        {
            return new CompilerException("Function must return a value");
        }
        
        internal static CompilerException MismatchedRaiseException()
        {
            return new CompilerException("Raise exception statement without an argument can appear only in Except block");
        }

    }

    public class ExtraClosedParenthesis : CompilerException
    {
        internal ExtraClosedParenthesis(CodePositionInfo codePosInfo) : base("Token expected: (")
        {
            this.LineNumber = codePosInfo.LineNumber;
            this.Code = codePosInfo.Code;
        }
    }

}
