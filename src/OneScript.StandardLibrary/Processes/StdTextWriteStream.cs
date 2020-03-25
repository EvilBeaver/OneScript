/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Processes
{
    /// <summary>
    /// Стандартный поток ввода текста. Используется для взаимодействия с работающими процессами.
    /// Методы работают подобно одноименным методам объекта ЗаписьТекста.
    /// </summary>
    [ContextClass("ПотокВводаТекста", "TextInputStream")]
    public class StdTextWriteStream : AutoContext<StdTextWriteStream>, IDisposable
    {
        private readonly StreamWriter _writer;

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
