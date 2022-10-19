/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Binary
{
    /// <summary>
    /// 
    /// Специализированная версия объекта Поток для работы данными, расположенными в файле на диске. Предоставляет возможность чтения из потока, записи в поток и изменения текущей позиции. 
    /// По умолчанию, все операции с файловым потоком являются буферизированными, размер буфера по умолчанию - 8 КБ.
    /// Размер буфера можно изменить, в том числе - полностью отключить буферизацию при вызове конструктора. 
    /// Следует учитывать, что помимо буферизации существует кэширование чтения и записи файлов в операционной системе, на которое невозможно повлиять программно.
    /// </summary>
    [ContextClass("ФайловыйПоток", "FileStream")]
    public class FileStreamContext : AutoContext<FileStreamContext>, IStreamWrapper, IDisposable
    {

        private readonly FileStream _underlyingStream;
        private readonly GenericStreamImpl _commonImpl;

        private FileShare FileShareForAccess(FileAccessEnum access)
        {
            return access == FileAccessEnum.Read ? FileShare.ReadWrite : FileShare.Read;
        }

        public FileStreamContext(string filename, FileOpenModeEnum openMode, FileAccessEnum access, int bufferSize = 0)
        {
            FileName = filename;

            if (bufferSize == 0)
                _underlyingStream = new FileStream(filename,
                    FileStreamsManager.ConvertFileOpenModeToCLR(openMode),
                    FileStreamsManager.ConvertFileAccessToCLR(access),
                    FileShareForAccess(access));
            else
                _underlyingStream = new FileStream(filename,
                    FileStreamsManager.ConvertFileOpenModeToCLR(openMode),
                    FileStreamsManager.ConvertFileAccessToCLR(access),
                    FileShareForAccess(access),
                    bufferSize);

            _commonImpl = new GenericStreamImpl(_underlyingStream);
        }

        public FileStreamContext(string fileName, FileStream openedStream)
        {
            FileName = fileName;
            _underlyingStream = openedStream;
            _commonImpl = new GenericStreamImpl(_underlyingStream);
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
        /// Признак доступности установки таймаута чтения/записи в потоке.
        /// </summary>
        /// <value>Булево (Boolean)</value>
        [ContextProperty("ДоступнаУстановкаТаймаута", "CanTimeout")]
        public bool CanTimeout => _underlyingStream.CanTimeout;

        /// <summary>
        /// 
        /// Время в миллисекундах, отведенное потоку на операцию чтения.
        /// </summary>
        /// <value>Число (int)</value>
        [ContextProperty("ТаймаутЧтения", "ReadTimeout")]
        public int ReadTimeout
        {
            get => _underlyingStream.ReadTimeout;
            set => _underlyingStream.ReadTimeout = value;
        }

        /// <summary>
        /// 
        /// Время в миллисекундах, отведенное потоку на операцию записи.
        /// </summary>
        /// <value>Число (int)</value>
        [ContextProperty("ТаймаутЗаписи", "WriteTimeout")]
        public int WriteTimeout
        {
            get => _underlyingStream.WriteTimeout;
            set => _underlyingStream.WriteTimeout = value;
        }

    /// <summary>
    /// Содержит полное имя файла, включая путь
    /// </summary>
    [ContextProperty("ИмяФайла")]
        public string FileName { get; }

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
        /// <returns name="number">
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
        /// <returns name="Stream"></returns>
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
        /// <returns name="number">
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

        public void Dispose()
        {
            Close();
        }

        public Stream GetUnderlyingStream()
        {
            return _underlyingStream;
        }

        [ScriptConstructor(Name = "С указанием режима открытия")]
        public static FileStreamContext Constructor(IValue filename, IValue openMode, IValue bufferSize = null)
        {
            if (bufferSize == null || bufferSize.SystemType == BasicTypes.Number)
            {
                return new FileStreamContext(
                    filename.AsString(),
                    ContextValuesMarshaller.ConvertParam<FileOpenModeEnum>(openMode),
                    FileAccessEnum.ReadAndWrite,
                    ContextValuesMarshaller.ConvertParam<int>(bufferSize));
            }
            else
            {
                // перегрузка методов не позволяет вызвать второй конструктор без доуточнения реальных типов
                return Constructor(
                    filename,
                    openMode,
                    new ClrEnumValueWrapper<FileAccessEnum>(null, FileAccessEnum.ReadAndWrite),
                    bufferSize);
            }
        }

        [ScriptConstructor(Name = "С указанием режима открытия и уровня доступа")]
        public static FileStreamContext Constructor(IValue filename, IValue openMode, IValue access, IValue bufferSize = null)
        {
            return new FileStreamContext(
                filename.AsString(),
                ContextValuesMarshaller.ConvertParam<FileOpenModeEnum>(openMode),
                ContextValuesMarshaller.ConvertParam<FileAccessEnum>(access),
                ContextValuesMarshaller.ConvertParam<int>(bufferSize));
        }
    }
}
