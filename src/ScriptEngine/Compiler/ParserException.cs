using System;

namespace ScriptEngine.Compiler
{
    public class ParserException : ScriptException
    {
        internal ParserException(CodePositionInfo posInfo, string message)
            :base(posInfo, message)
        {
        }

    }
}