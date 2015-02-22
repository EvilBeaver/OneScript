using System;
namespace OneScript.Compiler
{
    public interface ISourceCodeIndexer
    {
        string LineOfCode(int lineNumber);
    }
}
