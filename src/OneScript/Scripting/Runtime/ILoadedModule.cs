using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Scripting.Compiler;

namespace OneScript.Scripting.Runtime
{
    public interface ILoadedModule
    {
        string Name { get; set; }
        ISourceCodeIndexer SourceCodeIndexer { get; }
    }
}
