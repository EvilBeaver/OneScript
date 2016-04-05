using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Language;

namespace OneScript
{
    public interface ICompiledModule
    {
        string Name { get; set; }
        ISourceCodeIndexer SourceCodeIndexer { get; }

        string EntryPointName { get; }
    }
}
