using System;
namespace OneScript.Scripting
{
    public interface ISourceCodeIndexer
    {
        string LineOfCode(int lineNumber);
    }
}
