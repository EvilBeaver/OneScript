/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
            _path = System.IO.Path.GetFullPath(path);
        }

        private string GetCodeString()
        {
            if (_code == null)
            {
                var builder = new StringBuilder();
                using (var reader = FileOpener.OpenReader(_path))
                {
                    var buf = new char[2];
                    reader.Read(buf, 0, 2);
                    if (IsLinuxScript(buf))
                        reader.ReadLine();
                    else
                        builder.Append(buf);
                    
                    builder.Append(reader.ReadToEnd());

                    _code = builder.ToString();
                }
            }

            return _code;
        }

        private static bool IsLinuxScript(char[] buf)
        {
            return buf[0] == '#' && buf[1] == '!';
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
