/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Binary
{
    /// <summary>
    /// 
    /// Предоставляет методы для использования в типовых сценариях работы с файлами.
    /// </summary>
    [ContextClass("МенеджерФайловыхПотоков", "FileStreamsManager")]
    public class FileStreamsManager : AutoContext<FileStreamsManager>
    {

        public static FileMode ConvertFileOpenModeToCLR(FileOpenModeEnum value)
        {
            switch (value)
            {
                case FileOpenModeEnum.Append:
                    return FileMode.Append;
                case FileOpenModeEnum.Create:
                    return FileMode.Create;
                case FileOpenModeEnum.CreateNew:
                    return FileMode.CreateNew;
                case FileOpenModeEnum.Open:
                    return FileMode.Open;
                case FileOpenModeEnum.OpenOrCreate:
                    return FileMode.OpenOrCreate;
                case FileOpenModeEnum.Truncate:
                    return FileMode.Truncate;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public static FileAccess ConvertFileAccessToCLR(FileAccessEnum value)
        {
            switch (value)
            {
                case FileAccessEnum.Read:
                    return FileAccess.Read;
                case FileAccessEnum.Write:
                    return FileAccess.Write;
                default:
                    return FileAccess.ReadWrite;
            }
        }

        public FileStreamsManager()
        {
        }

        /// <summary>
        /// 
        /// Открывает файл в заданном режиме с возможностью чтения и записи. 
        /// Файл открывается в режиме разделяемого чтения.
        /// </summary>
        ///
        /// <param name="openingMode">
        /// Режим открытия файла. </param>
        /// <param name="fileName">
        /// Имя открываемого файла. </param>
        /// <param name="bufferSize">
        /// Размер буфера для операций с файлом. </param>
        ///
        /// <returns name="FileStream">
        /// Специализированная версия объекта Поток для работы данными, расположенными в файле на диске. Предоставляет возможность чтения из потока, записи в поток и изменения текущей позиции. 
        /// По умолчанию, все операции с файловым потоком являются буферизированными, размер буфера по умолчанию - 8 КБ.
        /// Размер буфера можно изменить, в том числе - полностью отключить буферизацию при вызове конструктора. 
        /// Следует учитывать, что помимо буферизации существует кэширование чтения и записи файлов в операционной системе, на которое невозможно повлиять программно.</returns>
        [DocumentedMember("Открыть", "Open")]
        public FileStreamContext Open(FileOpenModeEnum openingMode, string fileName, int bufferSize = 0)
        {
            return new FileStreamContext(fileName, openingMode, FileAccessEnum.ReadAndWrite, bufferSize);
        }
        
        /// <summary>
        /// 
        /// Открыть файл в выбранном режиме, с заданным уровнем доступа, с общим доступом на чтение.
        /// </summary>
        ///
        /// <param name="fileAccess">
        /// Режим доступа к файлу</param>
        /// <param name="fileName">
        /// Имя открываемого файла. </param>
        /// <param name="openingMode">
        /// Режим открытия файла. </param>
        /// <param name="bufferSize">
        /// Размер буфера для операций с файлом. </param>
        ///
        /// <returns name="FileStream">
        /// Специализированная версия объекта Поток для работы данными, расположенными в файле на диске. Предоставляет возможность чтения из потока, записи в поток и изменения текущей позиции. 
        /// По умолчанию, все операции с файловым потоком являются буферизированными, размер буфера по умолчанию - 8 КБ.
        /// Размер буфера можно изменить, в том числе - полностью отключить буферизацию при вызове конструктора. 
        /// Следует учитывать, что помимо буферизации существует кэширование чтения и записи файлов в операционной системе, на которое невозможно повлиять программно.</returns>
        [DocumentedMember("Открыть", "Open")]
        public FileStreamContext Open(FileAccessEnum fileAccess, string fileName, FileOpenModeEnum openingMode, int bufferSize = 0)
        {
            return new FileStreamContext(fileName, openingMode, fileAccess, bufferSize);
        }

        [ContextMethod("Открыть", "Open", SkipForDocumenter = true)]
        public FileStreamContext Open(IValue p1, IValue p2, IValue p3 = null, IValue p4 = null)
        {
            // Диспетчеризуем перегрузки по типам параметров
            if (p1 is ClrEnumValueWrapper<FileAccessEnum> fileAccess)
            {
                // это вариант с 4-мя параметрами
                return Open(
                    fileAccess.UnderlyingValue,
                    ContextValuesMarshaller.ConvertValueStrict<string>(p2),
                    ContextValuesMarshaller.ConvertValueStrict<FileOpenModeEnum>(p3),
                    ContextValuesMarshaller.ConvertValueStrict<int>(p4)
                );
            }
            else if (p4 == null)
            {
                // это вариант с 3-мя параметрами
                return Open(
                    ContextValuesMarshaller.ConvertValueStrict<FileOpenModeEnum>(p1),
                    ContextValuesMarshaller.ConvertValueStrict<string>(p2),
                    ContextValuesMarshaller.ConvertValueStrict<int>(p3)
                );
            }
            else
            {
                throw RuntimeException.InvalidNthArgumentType(4);
            }
        }

        /// <summary>
        /// 
        /// Открыть существующий файл для записи в конец. Если файл не существует, то будет создан новый файл. Запись в существующий файл выполняется с конца файла. Файл открывается в режиме разделяемого чтения.
        /// </summary>
        ///
        /// <param name="fileName">
        /// Имя открываемого файла. </param>
        /// 
        [ContextMethod("ОткрытьДляДописывания", "OpenForAppend")]
        public FileStreamContext OpenForAppend(string fileName)
        {
            return new FileStreamContext(fileName, FileOpenModeEnum.Append, FileAccessEnum.ReadAndWrite);
        }


        /// <summary>
        /// 
        /// Открывает существующий файл для записи. Файл открывается в режиме разделяемого чтения. Если файл не найден, будет создан новый файл. Запись в существующий файл производится с начала файла, замещая ранее сохраненные данные.
        /// </summary>
        ///
        /// <param name="fileName">
        /// Имя открываемого файла. </param>

        ///
        /// <returns name="FileStream">
        /// Специализированная версия объекта Поток для работы данными, расположенными в файле на диске. Предоставляет возможность чтения из потока, записи в поток и изменения текущей позиции. 
        /// По умолчанию, все операции с файловым потоком являются буферизированными, размер буфера по умолчанию - 8 КБ.
        /// Размер буфера можно изменить, в том числе - полностью отключить буферизацию при вызове конструктора. 
        /// Следует учитывать, что помимо буферизации существует кэширование чтения и записи файлов в операционной системе, на которое невозможно повлиять программно.</returns>

        ///
        [ContextMethod("ОткрытьДляЗаписи", "OpenForWrite")]
        public FileStreamContext OpenForWrite(string fileName)
        {
            // TODO: Судя по описанию - открывается без обрезки (Truncate). Надо проверить в 1С.
            return new FileStreamContext(fileName, FileOpenModeEnum.OpenOrCreate, FileAccessEnum.Write);
        }


        /// <summary>
        /// 
        /// Открывает существующий файл для чтения с общим доступом на чтение.
        /// </summary>
        ///
        /// <param name="fileName">
        /// Имя открываемого файла. </param>
        ///
        /// <returns name="FileStream"/>
        ///
        [ContextMethod("ОткрытьДляЧтения", "OpenForRead")]
        public FileStreamContext OpenForRead(string fileName)
        {
            return new FileStreamContext(fileName, FileOpenModeEnum.Open, FileAccessEnum.Read);
        }


        /// <summary>
        /// 
        /// Создает или перезаписывает файл и открывает поток с возможностью чтения и записи в файл. Файл открывается в режиме разделяемого чтения.
        /// </summary>
        ///
        /// <param name="fileName">
        /// Имя создаваемого файла. </param>
        /// <param name="bufferSize">
        /// Размер буфера. </param>
        ///
        /// <returns name="FileStream"/>
        [ContextMethod("Создать", "Create")]
        public IValue Create(string fileName, int bufferSize = 0)
        {
            return new FileStreamContext(fileName, new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, bufferSize == 0 ? 8192 : bufferSize));
        }


        /// <summary>
        /// НЕ РЕАЛИЗОВАН
        /// Создает временный файл и открывает его в монопольном режиме с возможностью чтения и записи. Дополнительно можно установить лимит в байтах, при превышении которого будет создан файл на диске. Пока размер файла не превышает данного лимита - вся работа ведётся в памяти.
        /// </summary>
        ///
        /// <param name="memoryLimit">
        /// Максимальный объем памяти (в байтах), при превышении которого будет создан файл на диске.
        /// Значение по умолчанию: 65535. </param>
        /// <param name="bufferSize">
        /// Размер буфера для операций с файлом (в байтах).
        /// Значение по умолчанию: 8192. </param>
        ///
        /// <returns name="FileStream"/>
        /// 
        [ContextMethod("СоздатьВременныйФайл", "CreateTempFile")]
        public IValue CreateTempFile(int memoryLimit = 0, int bufferSize = 0)
        {
            throw new NotImplementedException();
        }

    }
}
