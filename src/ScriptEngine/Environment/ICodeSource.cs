using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Environment
{
    public interface ICodeSource
    {
        ModuleHandle CreateModule();
        string SourceDescription { get; }
    }
 
}
