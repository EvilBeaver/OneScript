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
            using (var file = new System.IO.FileStream(_path, System.IO.FileMode.Open))
            {
                // *** Use Default of Encoding.Default (Ansi CodePage)
                Encoding enc = Encoding.Default;

                // *** Detect byte order mark if any - otherwise assume default
                byte[] buffer = new byte[5];

                file.Read(buffer, 0, 5);
                file.Position = 0;

                if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                    enc = Encoding.UTF8;
                else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                    enc = Encoding.Unicode;
                else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                    enc = Encoding.UTF32;
                else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                    enc = Encoding.UTF7;

                using (var reader = new System.IO.StreamReader(file, enc, true))
                {
                    return reader.ReadToEnd();
                }
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
