/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.StandardLibrary.Text;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Binary
{
    /// <summary>
    /// 
    /// Объект предназначен для чтения различных типов данных из источника данных. В качестве источника могут выступать Поток, Файл или ДвоичныеДанные.
    /// Необходимо соблюдать следующий порядок работы с данным объектом:
    /// 
    ///  - Создать объект ЧтениеДанных на основании источника данных.
    ///  - Выполнить требуемые действия с помощью объекта.
    ///  - Закрыть экземпляр объекта ЧтениеДанных.
    /// При необходимости использовать другие методы для работы с источником данных, требуется сначала закрыть экземпляр объекта ЧтениеДанных с помощью метода Закрыть, выполнить необходимые действия над источником, установить требуемую позицию для чтения из источника и создать новый экземпляр ЧтениеДанных.
    /// </summary>
    [ContextClass("ЧтениеДанных", "DataReader")]
    public class DataReader : AutoContext<DataReader>, IDisposable
    {
        private BinaryReader _reader;

        private Encoding _workingEncoding;
        private IValue _userVisibleEncoding;
        
        private DataReader(Stream stream, IValue textEncoding, ByteOrderEnum? byteOrder, string lineSplitter, string convertibleSplitterOfLines)
        {
            TextEncoding = textEncoding;
            ByteOrder = byteOrder ?? ByteOrderEnum.LittleEndian;
            LineSplitter = lineSplitter ?? "\n";
            ConvertibleSplitterOfLines = convertibleSplitterOfLines ?? "\r\n";
            _reader = new BinaryReader(stream, _workingEncoding);
        }

        /// <summary>
        /// 
        /// Создает объект для чтения из заданного объекта ДвоичныеДанные.
        /// После завершения работы с объектом ЧтениеДанных до того, как будет закрыт поток, переданный в конструктор, объект следует закрыть с помощью метода Закрыть или НачатьЗакрытие.
        /// </summary>
        ///
        /// <param name="dataSource">
        /// Путь к файлу или экземпляр объекта ДвоичныеДанные, из которого будет выполнено чтение. </param>
        /// <param name="textEncoding">
        /// Определяет кодировку текста, используемую для чтения данных. По-умолчанию используется кодировка UTF-8.
        /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
        /// 
        /// Значение по умолчанию: UTF8. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
        /// <param name="byteOrder">
        /// Порядок байтов, используемый для декодирования целых чисел при чтении из потока.
        /// Значение по умолчанию: LittleEndian. </param>
        /// <param name="lineSplitter">
        /// Определяет строку, разделяющую строки в двоичных данных.
        /// Значение по умолчанию: Неопределено. </param>
        /// <param name="convertibleSplitterOfLines">
        /// Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС.
        /// Значение по умолчанию: ВК + ПС. </param>
        [ScriptConstructor(Name = "На основании двоичных данных или имени файла")]
        public static DataReader Constructor(IValue dataSource, IValue textEncoding = null, ByteOrderEnum? byteOrder = null, string lineSplitter = "\n", string convertibleSplitterOfLines = null)
        {
            if (dataSource.SystemType == BasicTypes.String)
            {
                var stream = new FileStream(dataSource.AsString(), FileMode.Open, FileAccess.Read, FileShare.Read);
                return new DataReader(stream, textEncoding, byteOrder, lineSplitter, convertibleSplitterOfLines);
            }
            else
            {
                var obj = dataSource.AsObject();
                Stream stream;
                if (obj is BinaryDataContext)
                    stream = ((BinaryDataContext)obj).GetStream();
                else if (obj is IStreamWrapper)
                    stream = ((IStreamWrapper) obj).GetUnderlyingStream();
                else
                    throw RuntimeException.InvalidArgumentType("dataSource");

                return new DataReader(stream, textEncoding, byteOrder, lineSplitter, convertibleSplitterOfLines);
            }
        }
        
        /// <summary>
        /// 
        /// Кодировка текста, используемая по-умолчанию для данного экземпляра ЧтениеДанных.
        /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
        /// </summary>
        [ContextProperty("КодировкаТекста", "TextEncoding")]
        public IValue TextEncoding
        {
            get { return _userVisibleEncoding; }
            set
            {
                if (value != null)
                {
                    _workingEncoding = TextEncodingEnum.GetEncoding(value);
                    _userVisibleEncoding = value;
                }
                else
                {
                    _workingEncoding = new UTF8Encoding(true);
                    _userVisibleEncoding = ValueFactory.Create("utf-8");
                }
            }
        }
    
        /// <summary>
        /// 
        /// Конвертируемый разделитель строк. Этот параметр влияет на поведение метода ПрочитатьСимволы.
        /// </summary>
        /// <value>Строка (String)</value>
        [ContextProperty("КонвертируемыйРазделительСтрок", "ConvertibleSplitterOfLines")]
        public string ConvertibleSplitterOfLines { get; set; }
    
        /// <summary>
        /// 
        /// Порядок байтов по умолчанию.
        /// </summary>
        /// <value>ПорядокБайтов (ByteOrder)</value>
        [ContextProperty("ПорядокБайтов", "ByteOrder")]
        public ByteOrderEnum ByteOrder { get; set; }
    
        /// <summary>
        /// 
        /// Разделитель строк по-умолчанию. Это свойство влияет на поведение метода ПрочитатьСтроку.
        /// </summary>
        /// <value>Строка (String)</value>
        [ContextProperty("РазделительСтрок", "LineSplitter")]
        public string LineSplitter { get; set; }
    
        /// <summary>
        /// 
        /// Содержит признак того, что во входном потоке больше нет данных для чтения. Изначально устанавливается в значение Ложь. Если при очередном чтении было прочитано меньше данных, чем было запрошено, принимает значение Истина.
        /// </summary>
        /// <value>Булево (Boolean)</value>
        [ContextProperty("ЧтениеЗавершено", "ReadCompleted")]
        public bool ReadCompleted
        {
            // TODO: будет падать на непозиционируемых потоках
            get { return _reader.BaseStream.Position >= _reader.BaseStream.Length; }
        }
    
        /// <summary>
        /// 
        /// Вызов данного метода завершает работу с текущим объектом. Если объект является владельцем вложенного потока, поток также закрывается.
        /// </summary>
        ///
        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            _reader.Close();
        }


        /// <summary>
        /// 
        /// Получает исходный поток, из которого выполняется чтение данных.
        /// </summary>
        ///
        ///
        /// <returns name="Stream">
        /// </returns>
        ///
        [ContextMethod("ИсходныйПоток", "SourceStream")]
        public IValue SourceStream()
        {
            return new GenericStream(_reader.BaseStream);
        }
    
        /// <summary>
        /// 
        /// Пропускает при чтении указанное количество байтов в потоке.
        /// </summary>
        ///
        /// <param name="number">
        /// Количество байтов, которые требуется пропустить. </param>
        ///
        /// <returns name="Number">
        /// </returns>
        ///
        [ContextMethod("Пропустить", "Skip")]
        public long Skip(IValue value)
        {
            if (value.SystemType != BasicTypes.Number)
                throw RuntimeException.InvalidArgumentType();

            long number = (long)value.AsNumber();

            if (number < 0 || number != value.AsNumber())
                throw RuntimeException.InvalidArgumentValue();
            
            var stream = _reader.BaseStream;
            if (stream.CanSeek)
            {
                long bytesSkipped;
                var newPosition = stream.Position + number;
                if (newPosition > stream.Length)
                {
                    bytesSkipped = stream.Length - stream.Position;
                    stream.Seek(0, SeekOrigin.End);
                }
                else
                {
                    stream.Position += number;
                    bytesSkipped = number;
                }

                return bytesSkipped;
            }
            else
            {
                var buf = new byte[1024];
                var portion = (int)Math.Min(number, buf.Length);
                long bytesSkipped = 0;
                while (bytesSkipped < number)
                {
                    var read = stream.Read(buf, 0, portion);
                    bytesSkipped += read;
                    if (read < portion)
                        break;
                }

                return bytesSkipped;
            }
        }

        /// <summary>
        /// 
        /// Пропускает при чтении двоичные данные до указанного разделителя.
        /// </summary>
        ///
        /// <remarks>
        /// 
        /// Пропуск до двоичного маркера
        /// </remarks>
        ///
        /// <param name="marker">
        /// Маркер, до которого требуется пропустить данные. </param>
        ///
        /// <returns name="Number"/>
        ///
        [ContextMethod("ПропуститьДо", "SkipTo")]
        public int SkipTo(IValue marker)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// 
        /// Выполняет чтение двоичных данных заданного размера из потока. Если размер не указан, будут прочитаны все данные до конца потока.
        /// </summary>
        ///
        /// <param name="number">
        /// Количество байтов, которые требуется прочитать. Если не задано, то выполняется чтение всех данных до конца потока.
        /// Значение по умолчанию: Неопределено. </param>
        ///
        /// <returns name="ReadDataResult">
        /// Содержит описание результата чтения данных из потока.</returns>
        ///
        [ContextMethod("Прочитать", "Read")]
        public ReadDataResult Read(int number = 0)
        {
            var src = ReadSomeBytes(number);

            return new ReadDataResult(src.ToArray());
        }

        private MemoryStream ReadSomeBytes(int number)
        {
            MemoryStream src;
            if (number < 0)
                throw RuntimeException.InvalidArgumentValue();

            if (number == 0)
            {
                src = ReadAllData();
            }
            else
            {
                var dest = new byte[number];
                var actual = _reader.Read(dest, 0, number);
                src = new MemoryStream(dest, 0, actual);
            }
            return src;
        }

        private MemoryStream ReadAllData()
        {
            var destStream = new MemoryStream(4096);
            _reader.BaseStream.CopyTo(destStream);
            destStream.Position = 0;
            return destStream;
        }

        /// <summary>
        /// 
        /// Выполняет чтение одного байта из потока.
        /// </summary>
        ///
        /// <returns name="Number"/>
        /// 
        [ContextMethod("ПрочитатьБайт", "ReadByte")]
        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        /// <summary>
        /// 
        /// Прочитать байты из потока в БуферДвоичныхДанных.
        /// </summary>
        ///
        /// <remarks>
        /// 
        /// Чтение в существующий буфер
        /// </remarks>
        ///
        /// <param name="buffer">
        /// Буфер двоичных данных, в который требуется поместить прочитанные байты. 
        /// или Количество байтов, которые требуется прочитать (остальные параметры игнорируются).
        /// Если не задано, то выполняется чтение всех данных до конца потока. </param>
        /// <param name="positionInBuffer">
        /// Позиция в буфере, начиная с которой требуется записать прочитанные данные. </param>
        /// <param name="number">
        /// Количество байтов, которые требуется прочитать. </param>
        ///
        /// <returns name="BinaryDataBuffer"/>
        ///
        [ContextMethod("ПрочитатьВБуферДвоичныхДанных", "ReadIntoBinaryDataBuffer")]
        public IValue ReadIntoBinaryDataBuffer(IValue buffer=null, int positionInBuffer = 0, int number = 0)
        {
            if (buffer==null)
            {
                return new BinaryDataBuffer(ReadSomeBytes(0).ToArray());
            }
            else if (buffer.SystemType == BasicTypes.Number)
            {
                var stream = ReadSomeBytes((int)buffer.AsNumber());
                return new BinaryDataBuffer(stream.ToArray());
            }
            else
            {
                var binBuffer = (BinaryDataBuffer)buffer.AsObject();
                var stream = ReadSomeBytes(number);
                var bytesCount = number == 0 ? (int)stream.Length : number;
                stream.Read(binBuffer.Bytes, positionInBuffer, bytesCount);
                return buffer;
            }
        }


        /// <summary>
        /// 
        /// Выполняет чтение двоичных данных до указанного маркера.
        /// </summary>
        ///
        /// <remarks>
        /// 
        /// Чтение до двоичного маркера
        /// </remarks>
        ///
        /// <param name="marker">
        /// Маркер, до которого выполняется чтение данных. </param>
        ///
        /// <returns name="ReadDataResult">
        /// Содержит описание результата чтения данных из потока.</returns>
        ///
        [ContextMethod("ПрочитатьДо", "ReadTo")]
        public IValue ReadTo(IValue marker)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// Выполняет чтение заданного количества символов из потока в виде строки. Если количество символов не задано, выполняется чтение всего остатка потока.
        /// </summary>
        ///
        /// <param name="count">
        /// Количество символов, которые требуется прочитать. Если не установлено, то будут прочитаны все символы до конца потока.
        /// Значение по умолчанию: Неопределено. </param>
        /// <param name="encoding">
        /// Определяет кодировку текста. Если не установлена, используется кодировка, заданная для данного объекта ЧтениеДанных.
        /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
        /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
        ///
        /// <returns name="String"/>
        ///
        [ContextMethod("ПрочитатьСимволы", "ReadChars")]
        public string ReadChars(int count = 0, IValue encoding = null)
        {
            if (count == 0)
                count = (int)(_reader.BaseStream.Length - _reader.BaseStream.Position) * sizeof(char);

            char[] chars;
            if(encoding == null)
                chars = _reader.ReadChars(count);
            else
            {
                var enc = TextEncodingEnum.GetEncoding(encoding);
                _reader = new BinaryReader(_reader.BaseStream, enc);
                chars = _reader.ReadChars(count);
            }
            
            return new String(chars);
        }

        /// <summary>
        /// 
        /// Выполняет чтение строки текста до разделителя строки. Если разделитель не найден, чтение выполняется до конца потока.
        /// Если разделитель строки не задан явно, используется разделитель строки, указанный для данного экземпляра объекта ЧтениеДанных.
        /// </summary>
        ///
        /// <param name="encoding">
        /// Кодировка текста. Если не установлена, используется кодировка, заданная для текущего экземпляра ЧтениеДанных.
        /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
        /// <param name="lineSplitter">
        /// Указывает строку, являющуюся разделителем строк в читаемых данных. Если параметр не указан, используется разделитель строк, указанный для текущего экземпляра объекта ЧтениеДанных.
        /// Значение по умолчанию: Неопределено. </param>
        ///
        /// <returns name="String"/>
        ///
        [ContextMethod("ПрочитатьСтроку", "ReadLine")]
        public string ReadLine(IValue encoding = null, string lineSplitter = "\n")
        {
            var sr = new StreamReader(_reader.BaseStream);
            var textRdr = new CustomLineFeedStreamReader(sr, lineSplitter, false);
            return textRdr.ReadLine(lineSplitter ?? ConvertibleSplitterOfLines);
        }

        private T FromBytes<T>(byte[] bytes, Func<byte[], int, T> leConverter, Func<byte[], int, T> beConverter, IValue byteOrder = null)
        {
            ByteOrderEnum workByteOrder;
            if (byteOrder == null)
                workByteOrder = ByteOrder;
            else
            {
                var enumVal = byteOrder.GetRawValue() as IObjectWrapper;
                if (enumVal == null)
                    throw RuntimeException.InvalidArgumentType(nameof(byteOrder));

                try
                {
                    workByteOrder = (ByteOrderEnum)enumVal.UnderlyingObject;
                }
                catch (InvalidCastException)
                {
                    throw RuntimeException.InvalidArgumentType(nameof(byteOrder));
                }
            }

            var converter = workByteOrder == ByteOrderEnum.BigEndian ? beConverter : leConverter;
            return converter(bytes, 0);
        }

        /// <summary>
        /// 
        /// Считывает 16-битное целое число из потока.
        /// </summary>
        ///
        /// <param name="byteOrder">
        /// Порядок байтов, используемый при чтении числа.
        /// Если не задан, используется порядок, определенный для текущего экземпляра ЧтениеДанных.
        /// Значение по умолчанию: Неопределено. </param>
        ///
        /// <returns name="Number"/>
        ///
        [ContextMethod("ПрочитатьЦелое16", "ReadInt16")]
        public uint ReadInt16(IValue byteOrder = null)
        {
            var bytes = _reader.ReadBytes(sizeof(ushort));
            return FromBytes(bytes, BitConversionFacility.LittleEndian.ToUInt16, BitConversionFacility.BigEndian.ToUInt16, byteOrder);
        }


        /// <summary>
        /// 
        /// Прочитать 32-битное целое число из потока.
        /// </summary>
        ///
        /// <param name="byteOrder">
        /// Порядок байтов, используемый при чтении числа.
        /// Если не задан, используется порядок, определенный для текущего экземпляра ЧтениеДанных.
        /// Значение по умолчанию: Неопределено. </param>
        ///
        /// <returns name="Number">
        /// Числовым типом может быть представлено любое десятичное число. Над данными числового типа определены основные арифметические операции: сложение, вычитание, умножение и деление. Максимально допустимая разрядность числа 38 знаков.</returns>
        ///
        [ContextMethod("ПрочитатьЦелое32", "ReadInt32")]
        public uint ReadInt32(IValue byteOrder = null)
        {
            var bytes = _reader.ReadBytes(sizeof(uint));
            return FromBytes(bytes, BitConversionFacility.LittleEndian.ToUInt32, BitConversionFacility.BigEndian.ToUInt32, byteOrder);
        }


        /// <summary>
        /// 
        /// Считывает 64-битное целое число из потока.
        /// </summary>
        ///
        /// <param name="byteOrder">
        /// Устанавливает порядок байтов, используя который число будет прочитано. Если порядок байтов не задан, то используется порядок байтов, определенный для текущего экземпляра ЧтениеДанных.
        /// Значение по умолчанию: Неопределено. </param>
        ///
        /// <returns name="Number">
        /// Числовым типом может быть представлено любое десятичное число. Над данными числового типа определены основные арифметические операции: сложение, вычитание, умножение и деление. Максимально допустимая разрядность числа 38 знаков.</returns>
        ///
        [ContextMethod("ПрочитатьЦелое64", "ReadInt64")]
        public ulong ReadInt64(IValue byteOrder = null)
        {
            var bytes = _reader.ReadBytes(sizeof(ulong));
            return FromBytes(bytes, BitConversionFacility.LittleEndian.ToUInt64, BitConversionFacility.BigEndian.ToUInt64, byteOrder);
        }


        /// <summary>
        /// 
        /// Разделяет остаток данных по заданным разделителям.
        /// </summary>
        ///
        /// <remarks>
        /// 
        /// По двоичному разделителю
        /// </remarks>
        ///
        /// <param name="separator">
        /// Двоичный разделитель данных. </param>
        ///
        /// <returns name="Array"/>
        ///
        [ContextMethod("Разделить", "Split")]
        public IValue Split(IValue separator)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// Разделяет остаток данных по заданным разделителям.
        /// </summary>
        ///
        /// <remarks>
        /// 
        /// По строковому разделителю
        /// </remarks>
        ///
        /// <param name="separator">
        /// Разделитель данных в виде строки. </param>
        /// <param name="encoding">
        /// Кодировка текста. Если не задана, то используется кодировка, заданная для текущего экземпляра объекта ЧтениеДанных.
        /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
        ///
        /// <returns name="Array"/>
        ///
        [ContextMethod("Разделить", "Split")]
        public IValue Split(string separator, IValue encoding = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// Разделяет остаток данных на части заданного размера. Размер части указывается в байтах.
        /// </summary>
        ///
        /// <param name="partSizw">
        /// Размер части данных в байтах. </param>
        ///
        /// <returns name="Array"/>
        ///
        [ContextMethod("РазделитьНаЧастиПо", "SplitInPartsOf")]
        public IValue SplitInPartsOf(int partSizw)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
