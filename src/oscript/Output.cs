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

namespace oscript
{
    static class Output
    {
        public static Action<string> Write { get; private set; }
        private static Encoding _encoding;

        static Output()
        {
            Init();
        }

        public static ConsoleColor TextColor
        {
            get
            {
                return Console.ForegroundColor;
            }
            set
            {
                Console.ForegroundColor = value;
            }
        }

        private static void Init()
        {
            if (ConsoleOutputEncoding == null)
                Write = WriteStandardConsole;
            else
                Write = WriteEncodedStream;
        }

        public static void WriteLine(string text)
        {
            Write(text);
            WriteLine();
        }

        public static void WriteLine()
        {
            Write(Environment.NewLine);
        }

        private static void WriteStandardConsole(string text)
        {
            Console.Write(text);
        }

        private static void WriteEncodedStream(string text)
        {
            using (var stdout = Console.OpenStandardOutput())
            {
                var enc = ConsoleOutputEncoding;
                var bytes = enc.GetBytes(text);
                stdout.Write(bytes, 0, bytes.Length);
            }
        }

        public static Encoding ConsoleOutputEncoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                _encoding = value;
                Init();
            }
        }
    }
}
