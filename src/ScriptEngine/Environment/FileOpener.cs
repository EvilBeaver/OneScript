/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Text;

namespace ScriptEngine.Environment
{
    public static class FileOpener
    {
        public static StreamReader OpenReader(string filename)
        {
            return OpenReader(filename, FileShare.ReadWrite);
        }
        
        public static StreamReader OpenReader(string filename, FileShare shareMode, Encoding encoding = null)
        {
            var input = new FileStream(filename, FileMode.Open, FileAccess.Read, shareMode);

            if (encoding == null)
            {
                var enc = AssumeEncoding(input);
                return new StreamReader(input, enc, true);
            }

            return new StreamReader(input, encoding);
        }

        public static StreamReader OpenReader(string filename, Encoding encoding)
        {
            return OpenReader(filename, FileShare.ReadWrite, encoding);
        }

        public static StreamReader OpenReader(Stream stream, Encoding encoding = null)
        {
            if(encoding == null)
            {
                var enc = AssumeEncoding(stream);
                return new StreamReader(stream, enc, true);
            }
            return new StreamReader(stream, encoding);
        }

        public static StreamWriter OpenWriter(string filename)
        {
            var utf8BOMEncoding = new UTF8Encoding(true);
            return new StreamWriter(filename, false, utf8BOMEncoding);
        }

        public static StreamWriter OpenWriter(string filename, Encoding encoding)
        {
            return new StreamWriter(filename, false, encoding);
        }

        public static StreamWriter OpenWriter(string filename, Encoding encoding, bool append)
        {
            return new StreamWriter(filename, append, encoding);
        }

        public static Encoding AssumeEncoding(Stream inputStream)
        {
            return AssumeEncoding(inputStream, SystemSpecificEncoding());
        }

        public static Encoding SystemSpecificEncoding()
        {
            if(System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                return Encoding.UTF8;
            }
            else
            {
                return Encoding.Default;
            }
        }

        public static Encoding AssumeEncoding(Stream inputStream, Encoding fallbackEncoding)
        {
            var enc = fallbackEncoding;

            // *** Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[5];

            inputStream.Read(buffer, 0, 5);
            inputStream.Position = 0;

            if (buffer [0] == 0xef && buffer [1] == 0xbb && buffer [2] == 0xbf)
                enc = Encoding.UTF8;
            else if (buffer [0] == 0xfe && buffer [1] == 0xff)
                enc = Encoding.Unicode;
            else if (buffer [0] == 0 && buffer [1] == 0 && buffer [2] == 0xfe && buffer [3] == 0xff)
                enc = Encoding.UTF32;
            else if (buffer [0] == 0x2b && buffer [1] == 0x2f && buffer [2] == 0x76)
                enc = Encoding.UTF7;
            else if (buffer [0] == '#' && buffer [1] == '!') 
            {
                /* Если в начале файла присутствует shebang, считаем, что файл в UTF-8*/
                enc = Encoding.UTF8;
            }

            return enc;
        }

    }
}
