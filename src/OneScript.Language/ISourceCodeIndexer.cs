using System;
namespace OneScript.Language
{
    public interface ISourceCodeIndexer
    {
        string LineOfCode(int lineNumber);
    }
}
