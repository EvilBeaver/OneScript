using System;
namespace OneScript.Compiler
{
    public interface ILexemGenerator
    {
        string Code { get; set; }
        int CurrentColumn { get; }
        int CurrentLine { get; }
        CodePositionInfo GetCodePosition();
        Lexem NextLexem();
    }
}
