using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugServer
{
    static class SessionLog
    {
        private static StreamWriter _log;

        private static string _path;
        [Conditional("DEBUG")]
        public static void Open(string path)
        {
            _path = path;
            _log = new StreamWriter(path);
            _log.AutoFlush = true;
            _log.WriteLine("started: " + DateTime.Now);
        }

        public static void WriteLine(string text)
        {
#if DEBUG
            if (_log == null)
            {
                _log = new StreamWriter(_path);
                _log.AutoFlush = true;
                _log.WriteLine("started: " + DateTime.Now);
            }

            _log.WriteLine(text);
#endif
        }

        [Conditional("DEBUG")]
        public static void Close()
        {
            _log.WriteLine("closed: " + DateTime.Now);
            _log.Dispose();
            _log = null;
        }
    }
}
