/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.Environment
{
    public static class FileOpener
    {
        public static Encoding DefaultEncoding { get; set; }

        public static StreamReader OpenReader(string filename)
        {
            FileStream input = new FileStream(filename, FileMode.Open, FileAccess.Read);
            Encoding enc = AssumeEncoding(input);

			var reader = new StreamReader(input, enc, true);
			
			return reader;

        }

        public static StreamReader OpenReader(string filename, Encoding encoding)
        {
            return new StreamReader(filename, encoding);
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
            // *** Use Default of Encoding.Default (Ansi CodePage)
            var enc = DefaultEncoding == null ? Encoding.UTF8 : DefaultEncoding;

            return AssumeEncoding(inputStream, enc);

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
