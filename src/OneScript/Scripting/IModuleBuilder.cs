using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public interface IModuleBuilder
    {
        void BeginModule(CompilerContext context);
        void CompleteModule();
        void DefineVariable(string name);
        void DefineExportVariable(string name);

        void OnError(CompilerErrorEventArgs errorInfo);
    }
}
