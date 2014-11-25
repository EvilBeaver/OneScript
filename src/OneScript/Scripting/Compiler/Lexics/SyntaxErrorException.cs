using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler.Lexics
{
    public class SyntaxErrorException : ScriptException
    {
        internal SyntaxErrorException(CodePositionInfo codeInfo, string message):base(codeInfo, message)
        {

        }
    }
}
