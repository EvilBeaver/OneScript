using System;
namespace OneScript.Scripting.Compiler
{
    public interface ISourceCodeIndexer
    {
        string LineOfCode(int lineNumber);
    }
}
