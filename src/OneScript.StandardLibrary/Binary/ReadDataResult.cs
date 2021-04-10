/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Binary
{
    /// <summary>
    /// 
    /// Содержит описание результата чтения данных из потока.
    /// </summary>
    [ContextClass("РезультатЧтенияДанных", "ReadDataResult")]
    public class ReadDataResult : AutoContext<ReadDataResult>
    {
        private readonly byte[] _data;
        public ReadDataResult(byte[] data)
        {
            // Поиск по маркеру на данный момент не реализован.
            MarkerIndex = -1;
            MarkerFound = false;

            _data = data;
            Size = _data.Length;
        }
        
        /// <summary>
        /// 
        /// Индекс найденного маркера.
        /// </summary>
        /// <value>Число (Number)</value>
        [ContextProperty("ИндексМаркера", "MarkerIndex")]
        public int MarkerIndex { get; }
        
        /// <summary>
        /// 
        /// Содержит признак обнаружения маркера:
        /// 
        ///  - Истина - если в процессе чтения данных был обнаружен маркер.
        ///  - Ложь - если маркер не был найден или операция не предполагала поиска маркера.
        /// </summary>
        /// <value>Булево (Boolean)</value>
        [ContextProperty("МаркерНайден", "MarkerFound")]
        public bool MarkerFound { get; }
        
        /// <summary>
        /// 
        /// Размер данных в байтах. В некоторых случаях может быть равен нулю. Например, при чтении двоичных данных из конца потока или при разделении данных.
        /// </summary>
        /// <value>Число (Number)</value>
        [ContextProperty("Размер", "Size")]
        public int Size { get; }
        

        /// <summary>
        /// 
        /// Открывает поток для чтения данных.
        /// </summary>
        ///
        ///
        /// <returns name="Stream">
        /// Представляет собой поток данных, который можно последовательно читать и/или в который можно последовательно писать. 
        /// Экземпляры объектов данного типа можно получить с помощью различных методов других объектов.</returns>
        ///
        [ContextMethod("ОткрытьПотокДляЧтения", "OpenStreamForRead")]
        public GenericStream OpenStreamForRead()
        {
            var stream = new MemoryStream(_data);
            return new GenericStream(stream);
        }

        /// <summary>
        /// 
        /// Получает результат в виде буфера двоичных данных. Необходимо учитывать, что при этом данные будут полностью загружены в оперативную память. Если требуется избежать загрузки оперативной памяти, следует использовать другие методы получения двоичных данных.
        /// </summary>
        ///
        /// <returns name="BinaryDataBuffer">
        /// Коллекция байтов фиксированного размера с возможностью произвольного доступа и изменения по месту.</returns>
        ///
        [ContextMethod("ПолучитьБуферДвоичныхДанных", "GetBinaryDataBuffer")]
        public IValue GetBinaryDataBuffer()
        {
            // вроде бы, 1С делает копию данных для данного метода.
            // требуется уточнить правильное поведение
            return new BinaryDataBuffer((byte[])_data.Clone());
        }

        /// <summary>
        /// 
        /// Получает результат чтения в виде двоичных данных.
        /// </summary>
        ///
        ///
        /// <returns name="BinaryData"></returns>
        ///
        [ContextMethod("ПолучитьДвоичныеДанные", "GetBinaryData")]
        public IValue GetBinaryData()
        {
            return new BinaryDataContext(_data);
        }

    }
}
