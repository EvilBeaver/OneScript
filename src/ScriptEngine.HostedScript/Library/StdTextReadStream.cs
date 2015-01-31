using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("ПотокВыводаТекста","TextOutputStream")]
    public class StdTextReadStream : AutoContext<StdTextReadStream>, IDisposable
    {
        private StreamReader _reader;

        public StdTextReadStream(StreamReader source)
        {
            _reader = source;
        }

        [ContextProperty("ЕстьДанные", "HasData")]
        public bool HasData
        {
            get
            {
                return !_reader.EndOfStream;
            }
        }

        [ContextMethod("Прочитать", "Read")]
        public string Read()
        {
            return _reader.ReadToEnd();
        }
        
        [ContextMethod("ПрочитатьСтроку", "ReadLine")]
        public string ReadLine()
        {
            return _reader.ReadLine();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
