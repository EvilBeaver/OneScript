using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Environment;

namespace TestApp
{
    class EditedFileSource : ICodeSource
    {
        string _path = "";
        string _code = "";

        public EditedFileSource(string code, string path)
        {
            if (path != "")
                _path = System.IO.Path.GetFullPath(path);
            _code = code;
        }

        private string GetCodeString()
        {
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
            get
            { 
                if (_path != "")
                    return _path;
                return "<string>";
            }
        }

        #endregion
    }
}
