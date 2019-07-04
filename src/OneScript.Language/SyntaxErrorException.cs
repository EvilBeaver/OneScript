namespace OneScript.Language
{
    public class SyntaxErrorException : ScriptException
    {
        internal SyntaxErrorException(CodePositionInfo codeInfo, string message):base(codeInfo, message)
        {

        }
    }
}