using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    /// <summary>
    /// Стандартный поток ввода текста. Используется для взаимодействия с работающими процессами.
    /// Методы работают подобно одноименным методам объекта ЗаписьТекста.
    /// </summary>
    [ContextClass("ПотокВводаТекста", "TextInputStream")]
    public class StdTextWriteStream : AutoContext<StdTextWriteStream>, IDisposable
    {
        private StreamWriter _writer;

        public StdTextWriteStream(StreamWriter writer)
        {
            _writer = writer;
        }

        [ContextMethod("Записать","Write")]
        public void Write(string data)
        {
            _writer.Write(data);
        }

        [ContextMethod("ЗаписатьСтроку", "WriteLine")]
        public void WriteLine(string data)
        {
            _writer.WriteLine(data);
        }

        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            _writer.Close();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
