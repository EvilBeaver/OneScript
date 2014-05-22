using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine;

namespace oscript
{
    class ShowCompiledBehavior : AppBehavior
    {
        string _path;

        public ShowCompiledBehavior(string path)
        {
            _path = path;
        }

        public override int Execute()
        {
            var source = ScriptSourceFactory.FileBased(_path);
            var writer = new ScriptEngine.Compiler.ModuleWriter();
            writer.Write(Console.Out, source);
            return 0;
        }
    }
}
