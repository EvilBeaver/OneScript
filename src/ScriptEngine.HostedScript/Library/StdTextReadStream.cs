/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
    /// Стандартный поток вывода текста.
    /// </summary>
    [ContextClass("ПотокВыводаТекста","TextOutputStream")]
    public class StdTextReadStream : AutoContext<StdTextReadStream>, IDisposable
    {
        private StreamReader _reader;

        public StdTextReadStream(StreamReader source)
        {
            _reader = source;
        }

        /// <summary>
        /// Признак показывает, что в потоке есть данные.
        /// <example>
        /// Пока Поток.ЕстьДанные Цикл
        ///     Сообщить(Поток.ПрочитатьСтроку());
        /// КонецЦикла;
        /// </example>
        /// </summary>
        [ContextProperty("ЕстьДанные", "HasData")]
        public bool HasData
        {
            get
            {
                return !_reader.EndOfStream;
            }
        }

        /// <summary>
        /// Прочитать все данные из потока.
        /// </summary>
        /// <returns>Строка</returns>
        [ContextMethod("Прочитать", "Read")]
        public string Read()
        {
            return _reader.ReadToEnd();
        }
        
        /// <summary>
        /// Прочитать одну строку из потока.
        /// </summary>
        /// <returns>Строка</returns>
        [ContextMethod("ПрочитатьСтроку", "ReadLine")]
        public IValue ReadLine()
        {
            var readValue = _reader.ReadLine();
            return readValue == null ? ValueFactory.Create() : ValueFactory.Create(readValue);
        }

        /// <summary>
        /// Закрыть поток.
        /// </summary>
        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            _reader.Close();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
