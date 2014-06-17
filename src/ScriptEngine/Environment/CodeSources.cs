using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Compiler;

namespace ScriptEngine.Environment
{
    abstract class CodeSourceBase
    {
        private CompilerContext _symbols;

        public CodeSourceBase(CompilerContext symbols)
        {
            _symbols = symbols;
        }

        protected ModuleHandle CreateModule(ICodeSource src)
        {
            var loader = new TextCompiler(_symbols);
            var image = loader.Load(GetCodeString());
            return new ModuleHandle() { Module = image };
        }

        protected abstract string GetCodeString();

    }

    class StringBasedSource : CodeSourceBase, ICodeSource
    {
        string _src;

        public StringBasedSource(CompilerContext symbols, string src) : base(symbols)
        {
            _src = src;
        }

        #region ICodeSource Members

        ModuleHandle ICodeSource.CreateModule()
        {
            return base.CreateModule(this);
        }

        string ICodeSource.SourceDescription
        {
            get
            {
                return "<string>";
            }
        }

        #endregion

        protected override string GetCodeString()
        {
            return _src;
        }
    }

    class FileBasedSource : CodeSourceBase, ICodeSource
    {
        string _path;

        public FileBasedSource(CompilerContext symbols, string path) : base(symbols)
        {
            _path = path;
        }

        protected override string GetCodeString()
        {
            using (var reader = FileOpener.OpenReader(_path))
            {
                return reader.ReadToEnd();
            }
        }

        #region ICodeSource Members

        ModuleHandle ICodeSource.CreateModule()
        {
            return base.CreateModule(this);
        }

        string ICodeSource.SourceDescription
        {
            get { return _path; }
        }

        #endregion
    }
}
