/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using System.Text;

namespace OneScript.Sources
{
    public class FileCodeSource : ICodeSource
    {
        private readonly string _path;
        private readonly Encoding _noBomEncoding;

        public FileCodeSource(string path, Encoding defaultEncoding)
        {
            _path = Path.GetFullPath(path);
            _noBomEncoding = defaultEncoding;
        }

        public string GetSourceCode()
        {
            using (var fStream = new FileStream(_path, FileMode.Open, FileAccess.Read))
            {
                var buf = new byte[2];
                fStream.Read(buf, 0, 2);
                Encoding enc;
                var skipShebang = false;
                if (IsLinuxScript(buf))
                {
                    enc = Encoding.UTF8; // скрипты с shebang считать в формате UTF-8
                    skipShebang = true;
                }
                else
                {
                    fStream.Position = 0;
                    enc = FileOpener.AssumeEncoding(fStream, _noBomEncoding);
                }

                using (var reader = new StreamReader(fStream, enc))
                {
                    if (skipShebang)
                        reader.ReadLine();

                    return reader.ReadToEnd();
                }
            }
        }

        private static bool IsLinuxScript(byte[] buf)
        {
            return buf[0] == '#' && buf[1] == '!';
        }
        
        public string Location => _path[0].ToString().ToUpperInvariant() + _path.Substring(1);
    }
}