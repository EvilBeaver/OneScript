using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Compiler;

namespace ScriptEngine.Environment
{
    class ScriptSourceFactory : ICodeSourceFactory
    {
        public ICodeSource FromString(string source)
        {
            return new StringBasedSource(source);
        }

        public ICodeSource FromFile(string path)
        {
            return new FileBasedSource(path);
        }
    }
}
