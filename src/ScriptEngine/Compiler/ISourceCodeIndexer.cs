using System;
namespace ScriptEngine.Compiler
{
    interface ISourceCodeIndexer
    {
        string GetCodeLine(int index);
    }
}
