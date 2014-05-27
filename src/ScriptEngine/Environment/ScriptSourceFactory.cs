using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Compiler;

namespace ScriptEngine.Environment
{
    class ScriptSourceFactory : ICodeSourceFactory
    {
        private CompilerContext _symbols;

        public ScriptSourceFactory(CompilerContext symbols)
        {
            _symbols = symbols;
        }

        public ICodeSource FromString(string source)
        {
            return new StringBasedSource(_symbols, source);
        }

        public ICodeSource FromFile(string path)
        {
            return new FileBasedSource(_symbols, path);
        }
    }
}
