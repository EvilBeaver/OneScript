using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Compiler;

namespace ScriptEngine.Environment
{
    class StringBasedSource : ICodeSource
    {
        string _src;

        public StringBasedSource(string src)
        {
            _src = src;
        }

        #region ICodeSource Members

        string ICodeSource.Code
        {
            get
            {
                return _src;
            }
        }

        string ICodeSource.SourceDescription
        {
            get
            {
                return "<string>";
            }
        }

        #endregion

    }

    class FileBasedSource : ICodeSource
    {
        string _path;
        string _code;

        public FileBasedSource(string path)
        {
            _path = path;
        }

        private string GetCodeString()
        {
            if (_code == null)
            {
                using (var reader = FileOpener.OpenReader(_path))
                {
                    _code = reader.ReadToEnd();
                }
            }

            return _code;
        }

        #region ICodeSource Members

        string ICodeSource.Code
        {
            get
            {
                return GetCodeString();
            }
        }

        string ICodeSource.SourceDescription
        {
            get { return _path; }
        }

        #endregion
    }
}
