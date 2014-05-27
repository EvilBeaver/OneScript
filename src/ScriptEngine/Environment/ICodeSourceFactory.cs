using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Environment
{
    public interface ICodeSourceFactory
    {
        ModuleHandle FromFile(string path);
        ModuleHandle FromString(string code);
    }
}
