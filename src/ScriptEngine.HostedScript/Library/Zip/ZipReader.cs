/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Ionic.Zip;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Text;
using System.IO;
using ScriptEngine.HostedScript.Library.Binary;

namespace ScriptEngine.HostedScript.Library.Zip
{
    /// <summary>
    /// Объект чтения ZIP файлов.
    /// </summary>
    [ContextClass("ЧтениеZipФайла", "ZipFileReader")]
    public class ZipReader : AutoContext<ZipReader>, IDisposable
    {
        ZipFile _zip;
        ZipFileEntriesCollection _entriesWrapper;

        public ZipReader()
        {
        }
	    
        public ZipReader(IValue filenameOrStream, string password = null, FileNamesEncodingInZipFile encoding = FileNamesEncodingInZipFile.Auto)
        {
            Open(filenameOrStream, password, encoding);
        }

        private void CheckIfOpened()
        {
            if(_zip == null)
                throw new RuntimeException("Архив не открыт");
        }

        /// <summary>
        /// Открывает архив для чтения.
        /// </summary>
        /// <param name="filenameOrStream">Имя ZIP файла или Поток, который требуется открыть для чтения.</param>
        /// <param name="password">Пароль к файлу, если он зашифрован.</param>
        /// <param name="encoding">Кодировка имен файлов в архиве.</param>
        [ContextMethod("Открыть","Open")]
        public void Open(IValue filenameOrStream, string password = null, FileNamesEncodingInZipFile encoding = FileNamesEncodingInZipFile.Auto)
        {
            // fuck non-russian encodings on non-ascii files
            ZipFile.DefaultEncoding = Encoding.GetEncoding(866);
            
            if (filenameOrStream.DataType == DataType.String)
            {
                _zip = ZipFile.Read(filenameOrStream.AsString(), new ReadOptions { Encoding = ChooseEncoding(encoding) });
            } 
            else if (filenameOrStream.AsObject() is IStreamWrapper stream)
            {
                _zip = ZipFile.Read(stream.GetUnderlyingStream(), new ReadOptions { Encoding = ChooseEncoding(encoding) });
            } 
            else 
            {
                throw RuntimeException.InvalidArgumentType(nameof(filenameOrStream));
            }
            
            _zip.Password = password;
        }

        private Encoding ChooseEncoding(FileNamesEncodingInZipFile encoding)
        {
            if (encoding == FileNamesEncodingInZipFile.Auto || encoding == FileNamesEncodingInZipFile.OsEncodingWithUtf8) 
                return null;
            
            return Encoding.UTF8;

        }

        /// <summary>
        /// Извлечение всех файлов из архива
        /// </summary>
        /// <param name="where">Строка. Каталог в который извлекаются файлы</param>
        /// <param name="restorePaths">РежимВосстановленияПутейФайловZIP</param>
        [ContextMethod("ИзвлечьВсе","ExtractAll")]
        public void ExtractAll(string where, SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths = null)
        {
            CheckIfOpened();
            _zip.FlattenFoldersOnExtract = FlattenPathsOnExtraction(restorePaths);
            _zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
            _zip.ExtractAll(where);
        }

        /// <summary>
        /// Извлечение элемента из архива
        /// </summary>
        /// <param name="entry">ЭлементZipФайла. Извлекаемый элемент.</param>
        /// <param name="destination">Каталог, в который извлекается элемент.</param>
        /// <param name="restorePaths">РежимВосстановленияПутейФайлов</param>
        /// <param name="password">Пароль элемента (если отличается от пароля к архиву)</param>
        [ContextMethod("Извлечь", "Extract")]
        public void Extract(ZipFileEntryContext entry, string destination, SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths = null, string password = null)
        {
            CheckIfOpened();
            var realEntry = entry.GetZipEntry();
            _zip.FlattenFoldersOnExtract = FlattenPathsOnExtraction(restorePaths);
            realEntry.Password = password;

            using (FileStream streamToExtract = new FileStream(Path.Combine(destination, entry.Name), FileMode.Create))
            {
                realEntry.Extract(streamToExtract);
            }
        }

        /// <summary>
        /// Закрыть архив и освободить объект.
        /// </summary>
        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Коллекция элементов архива.
        /// </summary>
        [ContextProperty("Элементы", "Elements")]
        public ZipFileEntriesCollection Elements
        {
            get
            {
                CheckIfOpened();

                if (_entriesWrapper == null)
                    _entriesWrapper = new ZipFileEntriesCollection(_zip.Entries);

                return _entriesWrapper;
            }
        }

        private static bool FlattenPathsOnExtraction(SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths)
        {
            bool flattenFlag = false;
            if (restorePaths != null)
            {
                var zipEnum = (ZipRestoreFilePathsModeEnum)restorePaths.Owner;
                flattenFlag = restorePaths == zipEnum.DoNotRestore;
            }

            return flattenFlag;
        }

        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static ZipReader Construct()
        {
            return new ZipReader();
        }

        [ScriptConstructor(Name = "На основании имени файла или потока")]
        public static ZipReader Constructor(IValue dataSource, IValue password = null)
        {
            var dataSourceRawValue = dataSource.GetRawValue();

            return new ZipReader(dataSourceRawValue, password?.AsString());
        }

        public void Dispose()
        {
            _entriesWrapper = null;
            if (_zip != null)
            {
                _zip.Dispose();
                _zip = null;
            }
        }
    }
}
