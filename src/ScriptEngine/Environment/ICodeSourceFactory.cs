using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Environment
{
    public interface ICodeSourceFactory
    {
        ICodeSource FromFile(string path);
        ICodeSource FromString(string code);
    }
}
