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
using System.IO;

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
                using (var fStream = new FileStream(_path, FileMode.Open, FileAccess.Read))
                {
                    var buf = new byte[2];
                    fStream.Read(buf, 0, 2);
                    Encoding enc = null;
                    bool skipShebang = false;
                    if (IsLinuxScript(buf))
                    {
                        enc = Encoding.UTF8; // скрипты с shebang считать в формате UTF-8
                        skipShebang = true;
                    }
                    else
                    {
                        fStream.Position = 0;
                        enc = FileOpener.AssumeEncoding(fStream);
                    }

                    using (var reader = new StreamReader(fStream, enc))
                    {
                        if (skipShebang)
                            reader.ReadLine();

                        _code = reader.ReadToEnd();
                    }

                }
            }

            return _code;
        }

        private static bool IsLinuxScript(byte[] buf)
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
