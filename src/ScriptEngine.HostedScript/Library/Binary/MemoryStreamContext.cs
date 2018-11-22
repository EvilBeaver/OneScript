/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Binary
{
    /// <summary>
    /// 
    /// Представляет собой поток данных, который можно последовательно читать и/или в который можно последовательно писать. 
    /// Экземпляры объектов данного типа можно получить с помощью различных методов других объектов.
    /// </summary>
    [ContextClass("ПотокВПамяти", "MemoryStream")]
    public class MemoryStreamContext : AutoContext<MemoryStreamContext>, IDisposable, IStreamWrapper
    {
        private readonly bool _shouldBeCopiedOnClose;
        private readonly MemoryStream _underlyingStream;
        private readonly GenericStreamImpl _commonImpl;

        MemoryStreamContext()
        {
            _underlyingStream = new MemoryStream();
            _commonImpl = new GenericStreamImpl(_underlyingStream);
        }

        MemoryStreamContext(BinaryDataBuffer bytes)
        {
            _underlyingStream = new MemoryStream(bytes.Bytes);
            _shouldBeCopiedOnClose = !bytes.ReadOnly;
            _commonImpl = new GenericStreamImpl(_underlyingStream);
        }

        MemoryStreamContext(int capacity)
        {
            _underlyingStream = new MemoryStream(capacity);
            _commonImpl = new GenericStreamImpl(_underlyingStream);
        }

        /// <summary>
        /// 
        /// Создает поток, в качестве нижележащего хранилища для которого используется заданный байтовый буфер. Ёмкость потока ограничена размером буфера. При выходе за границы буфера будет сгенерировано исключение.
        /// Возможность записи в поток зависит от возможности изменения передаваемого буфера.
        /// </summary>
        ///
        /// <param name="bufferOrCapacity">
        /// Буфер, на основании которого будет создан поток или начальная емкость будущего потока. </param>
        ///
        [ScriptConstructor(Name = "По буферу или начальной емкости")]
        public static MemoryStreamContext Constructor(IValue bufferOrCapacity)
        {
            if (bufferOrCapacity.DataType == DataType.Number)
            {
                return new MemoryStreamContext((int)bufferOrCapacity.AsNumber());
            }

            var memBuf = ContextValuesMarshaller.ConvertParam<BinaryDataBuffer>(bufferOrCapacity);
            return new MemoryStreamContext(memBuf);
        }

        /// <summary>
        /// 
        /// Создает поток в памяти с расширяемой емкостью. Данный вариант можно использовать для работы с достаточно большими объемами данных, т.к. данные хранятся постранично, а не в виде одного последовательного блока.
        /// </summary>
        ///
        ///
        [ScriptConstructor]
        public static MemoryStreamContext Constructor()
        {
            return new MemoryStreamContext();
        }

        public bool IsReadOnly => !CanWrite;

        /// <summary>
        /// 
        /// Признак доступности записи в поток.
        /// </summary>
        /// <value>Булево (Boolean)</value>
        [ContextProperty("ДоступнаЗапись", "CanWrite")]
        public bool CanWrite => _underlyingStream.CanWrite;

        /// <summary>
        /// 
        /// Признак доступности произвольного изменения позиции чтения/записи в потоке.
        /// </summary>
        /// <value>Булево (Boolean)</value>
        [ContextProperty("ДоступноИзменениеПозиции", "CanSeek")]
        public bool CanSeek => _underlyingStream.CanSeek;


        /// <summary>
        /// 
        /// Признак доступности чтения из потока.
        /// </summary>
        /// <value>Булево (Boolean)</value>
        [ContextProperty("ДоступноЧтение", "CanRead")]
        public bool CanRead => _underlyingStream.CanRead;


        /// <summary>
        /// 
        /// Вызов данного метода завершает работу с потоком. При попытке вызвать любой метод объекта, кроме метода Закрыть, будет вызвано исключение. 
        /// При повторном вызове данного метода никаких действий выполняться не будет.
        /// Выполняемое действие зависит от используемого типа потока.
        /// </summary>
        ///
        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            _underlyingStream.Close();
        }


        /// <summary>
        /// 
        /// Записывает в поток заданное количество байтов из буфера по заданному смещению. Если в буфере меньше данных, чем требуется записать, вызывается исключение о недостаточном количестве данных в буфере.
        /// Запись в поток возможна только, если поток поддерживает запись. В противном случае при вызове метода будет вызвано исключение.
        /// </summary>
        ///
        /// <param name="buffer">
        /// Буфер, из которого выбираются данные для записи. </param>
        /// <param name="positionInBuffer">
        /// Позиция в буфере, начиная с которой данные будут получены для записи в поток. </param>
        /// <param name="number">
        /// Количество байт, которые требуется записать. </param>
        ///
        [ContextMethod("Записать", "Write")]
        public void Write(BinaryDataBuffer buffer, int positionInBuffer, int number)
        {
            _commonImpl.Write(buffer, positionInBuffer, number);
        }


        /// <summary>
        /// 
        /// Копирует данные из текущего потока в другой поток.
        /// </summary>
        ///
        /// <param name="targetStream">
        /// Поток, в который будет выполняться копирование. </param>
        /// <param name="bufferSize">
        /// Размер буфера, используемого при копировании.
        /// Если параметр не задан, то система подбирает размер буфера автоматически. </param>
        ///
        [ContextMethod("КопироватьВ", "CopyTo")]
        public void CopyTo(IValue targetStream, int bufferSize = 0)
        {
            _commonImpl.CopyTo(targetStream, bufferSize);
        }

        ///  <summary>
        ///  
        ///  Сдвигает текущую позицию потока на заданное количество байтов относительно начальной позиции. Если указано отрицательное смещение, позиция сдвигается в направлении к началу потока.
        ///  Если изменение позиции недоступно (ДоступноИзменениеПозиции установлено в Ложь), будет сгенерировано исключение.
        ///  </summary>
        /// 
        ///  <param name="offset">
        ///  Количество байтов, на которое нужно передвинуть позицию в потоке. </param>
        /// <param name="initialPosition">
        ///  Начальная позиция, от которой отсчитывается смещение. </param>
        /// <returns name="Number">
        ///  Числовым типом может быть представлено любое десятичное число. Над данными числового типа определены основные арифметические операции: сложение, вычитание, умножение и деление. Максимально допустимая разрядность числа 38 знаков.</returns>
        [ContextMethod("Перейти", "Seek")]
        public long Seek(int offset, StreamPositionEnum initialPosition = StreamPositionEnum.Begin)
        {
            return _commonImpl.Seek(offset, initialPosition);
        }


        /// <summary>
        /// 
        /// Возвращает поток, который разделяет данные и текущую позицию с данным потоком, но не разрешает запись.
        /// </summary>
        ///
        /// <returns name="Stream"/>
        ///
        [ContextMethod("ПолучитьПотокТолькоДляЧтения", "GetReadonlyStream")]
        public GenericStream GetReadonlyStream()
        {
            return _commonImpl.GetReadonlyStream();
        }


        /// <summary>
        /// 
        /// Выполняет чтение заданного количества байтов в указанный буфер по указанному смещению. Текущая позиция смещается вперед на фактическое количество прочитанных байтов.
        /// Чтение из потока возможно только, если поток поддерживает чтение. В противном случае, будет вызвано исключение.
        /// При чтении размер целевого буфера не меняется, а его содержимое перезаписывается фактически прочитанными данными. Если в буфере недостаточно места для записи прочитанных данных, происходит ошибка переполнения.
        /// </summary>
        ///
        /// <param name="buffer">
        /// Буфер, в который выполняется чтение. </param>
        /// <param name="positionInBuffer">
        /// Позиция в целевом буфере, начиная с которой требуется записывать данные из потока. </param>
        /// <param name="number">
        /// Количество байт, которые требуется записать в целевой буфер. </param>
        ///
        /// <returns name="number">
        /// Возвращает число прочитанных байт
        /// </returns>
        /// 
        [ContextMethod("Прочитать", "Read")]
        public long Read(BinaryDataBuffer buffer, int positionInBuffer, int number)
        {
            return _commonImpl.Read(buffer, positionInBuffer, number);
        }


        /// <summary>
        /// 
        /// Получает размер данных в байтах.
        /// </summary>
        ///
        [ContextMethod("Размер", "Size")]
        public long Size()
        {
            return _commonImpl.Size();
        }


        /// <summary>
        /// 
        /// Сбрасывает все промежуточные буферы и производит запись всех незаписанных данных в целевое устройство.
        /// </summary>
        ///
        [ContextMethod("СброситьБуферы", "Flush")]
        public void Flush()
        {
            _commonImpl.Flush();
        }


        /// <summary>
        /// 
        /// Возвращает текущую позицию в потоке.
        /// </summary>
        ///

        ///
        /// <returns name="Number">
        /// Числовым типом может быть представлено любое десятичное число. Над данными числового типа определены основные арифметические операции: сложение, вычитание, умножение и деление. Максимально допустимая разрядность числа 38 знаков.</returns>

        ///
        [ContextMethod("ТекущаяПозиция", "CurrentPosition")]
        public long CurrentPosition()
        {
            return _commonImpl.CurrentPosition();
        }


        /// <summary>
        /// 
        /// Устанавливает размер потока.
        /// Если текущий размер превышает заданный, поток будет сокращен до заданного размера, а информация, превышающая заданный размер, будет потеряна.
        /// Если текущий размер потока меньше заданного, то содержимое потока между старым и новым размером не определено.
        /// </summary>
        ///
        /// <param name="size">
        /// Устанавливаемый размер потока. </param>
        /// 
        [ContextMethod("УстановитьРазмер", "SetSize")]
        public void SetSize(long size)
        {
            _commonImpl.SetSize(size);
        }

        /// <summary>
        /// закрывает поток и возвращает результат в виде двоичных данных
        /// </summary>
        /// <returns></returns>
        [ContextMethod("ЗакрытьИПолучитьДвоичныеДанные")]
        public BinaryDataContext CloseAndGetBinaryData()
        {
            byte[] bytes;
            if (_shouldBeCopiedOnClose)
            {
                _underlyingStream.Position = 0;
                bytes = new byte[_underlyingStream.Length];
                _underlyingStream.Read(bytes, 0, bytes.Length);

            }
            else
            {
                bytes = _underlyingStream.GetBuffer();
            }

            _underlyingStream.Close();

            return new BinaryDataContext(bytes);
        }

        public void Dispose()
        {
            Close();
        }

        public Stream GetUnderlyingStream()
        {
            return _underlyingStream;
        }
    }
}
