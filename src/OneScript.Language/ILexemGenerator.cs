using System;
namespace OneScript.Language
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
