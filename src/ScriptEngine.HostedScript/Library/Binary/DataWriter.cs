/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Binary
{
    /// <summary>
    /// 
    /// Объект предназначен для удобной записи различных типов данных в приемник.
    /// Необходимо соблюдать следующий порядок работы с данным объектом:
    /// 
    ///  - Создать или получить приемник даных.
    ///  - Вызвать из приемника экземпляр объекта ЗаписьДанных.
    ///  - Выполнить требуемые действия с помощью объекта.
    ///  - Закрыть экземпляр объекта ЗаписьДанных.
    /// При необходимости использовать другие методы для работы с данными, требуется сначала закрыть экземпляр объекта ЗаписьДанных с помощью метода Закрыть, выполнить необходимые действия над данными, установить требуемую позицию для чтения из приемника и создать новый экземпляр ЗаписьДанных.
    /// </summary>
    [ContextClass("ЗаписьДанных", "DataWriter")]
    public class DataWriter : AutoContext<DataWriter>, IDisposable
    {

        private Encoding _workingEncoding;
        private IValue _userVisibleEncoding;
        private BinaryWriter _binaryWriter;
        private readonly bool _writeBOM;
    
        public DataWriter(string fileName, IValue textEncoding, ByteOrderEnum? byteOrder, string lineSplitter, bool append, string convertibleSplitterOfLines, bool writeBOM)
        {
            ByteOrder = byteOrder?? ByteOrderEnum.LittleEndian;
            LineSplitter = lineSplitter?? "\r\n";
            ConvertibleSplitterOfLines = convertibleSplitterOfLines;
            _writeBOM = writeBOM;
            TextEncoding = textEncoding;

            var fileSubsystem = new FileStreamsManager();
            var fileStreamContext = append ? fileSubsystem.OpenForAppend(fileName) : fileSubsystem.OpenForWrite(fileName);

            _binaryWriter = new BinaryWriter(fileStreamContext.GetUnderlyingStream(), _workingEncoding);
        }

        public DataWriter(IStreamWrapper streamObj, IValue textEncoding, ByteOrderEnum? byteOrder, string lineSplitter, string convertibleSplitterOfLines, bool writeBOM)
        {
            ByteOrder = byteOrder?? ByteOrderEnum.LittleEndian;
            LineSplitter = lineSplitter ?? "\r\n";
            ConvertibleSplitterOfLines = convertibleSplitterOfLines;
            _writeBOM = writeBOM;
            TextEncoding = textEncoding;

            _binaryWriter = new BinaryWriter(streamObj.GetUnderlyingStream(), _workingEncoding, true);
        }

        /// <summary>
        /// 
        /// Создает объект ЗаписьДанных для записи в указанный файл. Если файл с таким именем не существует, он будет создан. Параметр &lt;Дописать&gt; определяет, будут ли данные записаны в начало или в конец файла.
        /// После завершения работы с объектом, до закрытия потока, переданного в конструктор, объект необходимо закрыть с помощью метода Закрыть или НачатьЗакрытие. При этом используемый файл будет закрыт автоматически.
        /// </summary>
        ///
        /// <param name="file_stream">
        /// Имя файла или поток, в который будет выполнена запись. </param>
        /// <param name="textEncoding">
        /// Кодировка текста для создаваемого экземпляра ЗаписьДанных. Если не задана, то используется UTF-8.
        /// Значение по умолчанию: UTF8. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
        /// <param name="byteOrder">
        /// Порядок байтов, используемый по умолчанию для кодирования целых чисел при записи в поток.
        /// Значение по умолчанию: LittleEndian. </param>
        /// <param name="lineSplitter">
        /// Разделитель по умолчанию для строк, записываемых в поток. Если разделитель строк не задан, то используется строка ПС.
        /// Значение по умолчанию: ПС. </param>
        /// <param name="param5">
        /// Для файла:
        ///  Определяет, будут ли данные записаны в начало или в конец файла:
        ///  - Если Истина, то при открытии существующего файла запись будет выполнена в конец файла.
        ///  - Иначе данные будут записываться с начала файла, перезаписывая существующие данные.
        ///  Если заданный файл не существует, будет создан новый файл с указанным именем и значение параметра не повлияет на поведение конструктора.
        ///  Значение по умолчанию: Ложь.
        /// Для потока:
        ///  Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС.
        ///  Значение по умолчанию: ВК + ПС. </param>
        /// <param name="param6">
        /// Для файла:
        ///  Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС.
        ///  Значение по умолчанию: ВК + ПС.
        /// Для потока:
        ///  Если в начало файла или потока требуется записать метку порядка байтов (BOM) для используемой кодировки текста, то данный параметр должен иметь значение Истина.
        ///  Значение по умолчанию: Ложь. </param>
        /// <param name="param7">
        /// Только для файла:
        ///  Если в начало файла требуется записать метку порядка байтов (BOM) для используемой кодировки текста, то данный параметр должен иметь значение Истина.
        ///  Значение по умолчанию: Ложь. </param>
        ///
        [ScriptConstructor]
        public static DataWriter Constructor(IValue file_stream, IValue textEncoding = null, IValue byteOrder = null, IValue lineSplitter = null, IValue param5 = null, IValue param6 = null, IValue param7 = null)
        {
            if (file_stream.DataType == DataType.String)
                return new DataWriter(file_stream.AsString(), textEncoding, 
                            ContextValuesMarshaller.ConvertParam<ByteOrderEnum?>(byteOrder,null),
                            ContextValuesMarshaller.ConvertParam<string>(lineSplitter),
                            ContextValuesMarshaller.ConvertParam<bool>(param5),
                            ContextValuesMarshaller.ConvertParam<string>(param6),
                            ContextValuesMarshaller.ConvertParam<bool>(param7));
            else
                return ConstructorByStream(file_stream, textEncoding,
                            ContextValuesMarshaller.ConvertParam<ByteOrderEnum?>(byteOrder,null),
                            ContextValuesMarshaller.ConvertParam<string>(lineSplitter),
                            ContextValuesMarshaller.ConvertParam<string>(param5),
                            ContextValuesMarshaller.ConvertParam<bool>(param6));
        }

        /// <summary>
        /// 
        /// Объект создается для записи в заданный поток.
        /// После завершения работы с объектом, до закрытия потока, переданного в конструктор, объект необходимо закрыть с помощью метода Закрыть или НачатьЗакрытие.
        /// </summary>
        ///
        /// <param name="stream">
        /// Поток, в который производится запись данных. Типы: Поток (Stream), ПотокВПамяти (MemoryStream), ФайловыйПоток (FileStream) </param>
        /// <param name="textEncoding">
        /// Устанавливает кодировку текста для создаваемого экземпляра ЗаписьДанных. Если не задано, используется кодировка UTF-8.
        /// Значение по умолчанию: UTF8. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
        /// <param name="byteOrder">
        /// Порядок байтов, используемый по умолчанию для кодирования целых чисел при записи в поток.
        /// Значение по умолчанию: LittleEndian. </param>
        /// <param name="lineSplitter">
        /// Разделитель по умолчанию для строк, записываемых в поток. Если разделитель строк не задан, то используется строка ПС.
        /// Значение по умолчанию: ПС. </param>
        /// <param name="convertibleSplitterOfLines">
        /// Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС.
        /// Значение по умолчанию: ВК + ПС. </param>
        /// <param name="writeBOM">
        /// Если в начало файла или потока требуется записать метку порядка байтов (BOM) для используемой кодировки текста, то данный параметр должен иметь значение Истина.
        /// Значение по умолчанию: Ложь. </param>
        ///
        //[ScriptConstructor(Name = "На основании потока")]
        public static DataWriter ConstructorByStream(IValue stream, IValue textEncoding = null, ByteOrderEnum? byteOrder = null, string lineSplitter = null, string convertibleSplitterOfLines = null, bool writeBOM = false)
        {
            var streamObj = stream.AsObject() as IStreamWrapper;
            if (streamObj == null)
            {
                throw RuntimeException.InvalidArgumentType(nameof(stream));
            }

            return new DataWriter(streamObj, textEncoding, byteOrder, lineSplitter, convertibleSplitterOfLines, writeBOM);
        }

        /// <summary>
        /// 
        /// Кодировка текста по-умолчанию для данного экземпляра ЗаписьДанных.
        /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
        /// </summary>
        /// <value>КодировкаТекста (TextEncoding), Строка (String)</value>
        [ContextProperty("КодировкаТекста", "TextEncoding")]
        public IValue TextEncoding
        {
            get { return _userVisibleEncoding; }
            set
            {
                if (value != null)
                {
                    _workingEncoding = TextEncodingEnum.GetEncoding(value, _writeBOM);
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
        /// Конвертируемый разделитель строк. Этот параметр влияет на поведение метода ЗаписатьСимволы.
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
        public ByteOrderEnum ByteOrder { get; private set; }


        /// <summary>
        /// 
        /// Разделитель строк по-умолчанию. Это свойство влияет на поведение метода ЗаписатьСтроку.
        /// </summary>
        /// <value>Строка (String)</value>
        [ContextProperty("РазделительСтрок", "LineSplitter")]
        public string LineSplitter { get; set; }
    
        /// <summary>
        /// 
        /// Вызывает метод СброситьБуферы. Если целевой поток был создан при создании объекта ЗаписьДанных, целевой поток также закрывается.
        /// </summary>
        ///
        ///
        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            _binaryWriter.Close();
        }


        /// <summary>
        /// 
        /// Записывает данные в целевой поток.
        /// </summary>
        ///
        /// <remarks>
        /// 
        /// Запись двоичных данных
        /// </remarks>
        ///
        /// <param name="binaryDataOrReadResult">
        /// Записать экземпляр объекта ДвоичныеДанные в поток. </param>
        ///
        [ContextMethod("Записать", "Write")]
        public void Write(IValue binaryDataOrReadResult)
        {
            var binData = binaryDataOrReadResult.AsObject() as BinaryDataContext;
            if (binData == null) //TODO: Поддержкать класс РезультатЧтенияДанных
                throw RuntimeException.InvalidArgumentType();

            binData.CopyTo(_binaryWriter.BaseStream);
        }
    
        /// <summary>
        /// 
        /// Записывает один байт в целевой поток.
        /// </summary>
        ///
        /// <param name="number">
        /// Целое число, которое будет записано в целевой поток. Значение числа должно находиться в диапазоне от 0 до 255. </param>
        ///
        [ContextMethod("ЗаписатьБайт", "WriteByte")]
        public void WriteByte(byte number)
        {
            _binaryWriter.Write(number);
        }
    
        /// <summary>
        /// 
        /// Записать байты из буфера двоичных данных в целевой поток.
        /// </summary>
        ///
        /// <remarks>
        /// 
        /// Запись части байтов из буфера
        /// </remarks>
        ///
        /// <param name="buffer">
        /// Буфер двоичных данных, который используется в качестве источника данных для записи в целевой поток. </param>
        /// <param name="positionInBuffer">
        /// Позиция в буфере, начиная с которой выполняется чтение байтов для записи в целевой поток. </param>
        /// <param name="number">
        /// Количество байтов, которые требуется записать в целевой поток. </param>
        ///
        [ContextMethod("ЗаписатьБуферДвоичныхДанных", "WriteBinaryDataBuffer")]
        public void WriteBinaryDataBuffer(BinaryDataBuffer buffer, int positionInBuffer = 0, int number = 0)
        {
            if(positionInBuffer == 0 && number == 0)
                _binaryWriter.Write(buffer.Bytes, 0, buffer.Count());
            else
                _binaryWriter.Write(buffer.Bytes, positionInBuffer, number);
        }


        /// <summary>
        /// 
        /// Записывает символы заданной строки в целевой поток.
        /// </summary>
        ///
        /// <param name="line">
        /// Строка, символы которой будут записаны в поток. </param>
        /// <param name="encoding">
        /// Определяет кодировку текста для записи строки. Если не установлена, используется кодировка, заданная для данного объекта ЗаписьДанных.
        /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.</param>
        ///
        [ContextMethod("ЗаписатьСимволы", "WriteChars")]
        public void WriteChars(string line, IValue encoding = null)
        {
            if(encoding == null)
                _binaryWriter.Write(line.ToCharArray());
            else
            {
                var enc = TextEncodingEnum.GetEncoding(encoding, _writeBOM);
                var bytes = enc.GetBytes(line);
                _binaryWriter.Write(bytes,0,bytes.Length);
            }
        }


        /// <summary>
        /// 
        /// Записывает строку в целевой поток.
        /// Сначала записываются все символы строки, затем - разделитель строк.
        /// </summary>
        ///
        /// <param name="line">
        /// Строка, которая будет записана в поток. </param>
        /// <param name="encoding">
        /// Определяет кодировку текста для записи строки. Если не установлена, используется кодировка, заданная для данного объекта ЗаписьДанных.
        /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
        /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
        /// <param name="lineSplitter">
        /// Указывает строку, являющуюся разделителем строк в потоке после записи символов строк. Если параметр не указан, используется разделитель строк, указанный для текущего экземпляра объекта ЗаписьДанных.
        /// Значение по умолчанию: Неопределено. </param>
        ///
        [ContextMethod("ЗаписатьСтроку", "WriteLine")]
        public void WriteLine(string line, IValue encoding = null, string lineSplitter = null)
        {
            // TODO: Для экономии времени не поддерживаем пока конвертируемый разделитель строк
            // Кому надо - попросит PR.

            if (encoding == null)
                _binaryWriter.Write(line.ToCharArray());
            else
            {
                var enc = TextEncodingEnum.GetEncoding(encoding, _writeBOM);
                var bytes = enc.GetBytes(line);
                _binaryWriter.Write(bytes, 0, bytes.Length);
            }

            if(lineSplitter == null)
                _binaryWriter.Write(LineSplitter.ToCharArray());
            else
                _binaryWriter.Write(lineSplitter.ToCharArray());
        }

        private byte[] GetBytes<T>(T value, Converter<T, byte[]> leConverter, Converter<T, byte[]> beConverter, IValue byteOrder = null)
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
                    workByteOrder = (ByteOrderEnum) enumVal.UnderlyingObject;
                }
                catch (InvalidCastException)
                {
                    throw RuntimeException.InvalidArgumentType(nameof(byteOrder));
                }
            }

            var converter = workByteOrder == ByteOrderEnum.BigEndian ? beConverter : leConverter;
            return converter(value);
        }
    
        /// <summary>
        /// 
        /// Записывает 16-разрядное число в целевой поток.
        /// </summary>
        ///
        /// <param name="number">
        /// Число, которое будет записано в целевой поток.
        /// Значение числа должно находиться в диапазоне от 0 до 65535. </param>
        /// <param name="byteOrder">
        /// Порядок байтов, который будет использован для кодировки числа при записи. Если не установлен, то будет использован порядок байтов, заданный для текущего экземпляра объекта ЗаписьДанных.
        /// Значение по умолчанию: Неопределено. </param>
        ///
        [ContextMethod("ЗаписатьЦелое16", "WriteInt16")]
        public void WriteInt16(ushort number, IValue byteOrder = null)
        {
            var buffer = GetBytes(number, BitConversionFacility.LittleEndian.GetBytes, BitConversionFacility.BigEndian.GetBytes, byteOrder);
            _binaryWriter.Write(buffer, 0, buffer.Length);
        }
    
        /// <summary>
        /// 
        /// Записать целое 32-битное число в целевой поток.
        /// </summary>
        ///
        /// <param name="number">
        /// Целое число, которое будет записано в целевой поток. Значение числа должно находиться в диапазоне от 0 до 2^32-1. </param>
        /// <param name="byteOrder">
        /// Порядок байтов, который будет использован для кодировки числа при записи. Если не установлен, то будет использован порядок байтов, заданный для текущего экземпляра объекта ЗаписьДанных.
        /// Значение по умолчанию: Неопределено. </param>
        ///
        [ContextMethod("ЗаписатьЦелое32", "WriteInt32")]
        public void WriteInt32(uint number, IValue byteOrder = null)
        {
            var buffer = GetBytes(number, BitConversionFacility.LittleEndian.GetBytes, BitConversionFacility.BigEndian.GetBytes, byteOrder);
            _binaryWriter.Write(buffer, 0, buffer.Length);
        }


        /// <summary>
        /// 
        /// Записывает целое 64-битное число в целевой поток.
        /// </summary>
        ///
        /// <param name="number">
        /// Целое число, которое будет записано в целевой поток. Значение числа должно находиться в диапазоне от 0 до 2^64-1. </param>
        /// <param name="byteOrder">
        /// Порядок байтов, который будет использован для кодировки числа при записи. Если не установлен, то будет использован порядок байтов, заданный для текущего экземпляра объекта ЗаписьДанных.
        /// Значение по умолчанию: Неопределено. </param>
        ///
        [ContextMethod("ЗаписатьЦелое64", "WriteInt64")]
        public void WriteInt64(ulong number, IValue byteOrder = null)
        {
            var buffer = GetBytes(number, BitConversionFacility.LittleEndian.GetBytes, BitConversionFacility.BigEndian.GetBytes, byteOrder);
            _binaryWriter.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 
        /// Сбрасывает все внутренние буферы в целевой поток, после чего вызывает метод СброситьБуферы целевого потока.
        /// </summary>
        ///
        [ContextMethod("СброситьБуферы", "Flush")]
        public void Flush()
        {
            _binaryWriter.Flush();
        }


        /// <summary>
        /// 
        /// Возвращает целевой поток, в который выполняется запись.
        /// </summary>
        ///
        ///
        /// <returns name="Stream"/>
        ///
        [ContextMethod("ЦелевойПоток", "TargetStream")]
        public IValue TargetStream()
        {
            return new GenericStream(_binaryWriter.BaseStream);
        }

        public void Dispose()
        {
            _binaryWriter.Dispose();
        }
    }
}
