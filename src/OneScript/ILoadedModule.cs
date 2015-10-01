using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Language;

namespace OneScript
{
    public interface ILoadedModule
    {
        string Name { get; set; }
        ISourceCodeIndexer SourceCodeIndexer { get; }
    }
}
