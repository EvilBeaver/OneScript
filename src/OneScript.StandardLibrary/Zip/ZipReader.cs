/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Text;
using Ionic.Zip;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.IO;
using OneScript.Core;

namespace OneScript.StandardLibrary.Zip
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
	    
        public ZipReader(string filename, string password = null)
        {
            Open(filename, password);
        }

        private void CheckIfOpened()
        {
            if(_zip == null)
                throw new RuntimeException("Архив не открыт");
        }

        /// <summary>
        /// Открывает архив для чтения.
        /// </summary>
        /// <param name="filename">Имя ZIP файла, который требуется открыть для чтения.</param>
        /// <param name="password">Пароль к файлу, если он зашифрован.</param>
        /// <param name="encoding">Кодировка имен файлов в архиве.</param>
        [ContextMethod("Открыть","Open")]
        public void Open(string filename, string password = null, FileNamesEncodingInZipFile encoding = FileNamesEncodingInZipFile.Auto)
        {
            ZipFile.DefaultEncoding = Encoding.GetEncoding(866);
            // fuck non-russian encodings on non-ascii files
            _zip = ZipFile.Read(filename, new ReadOptions() { Encoding = ChooseEncoding(encoding) });
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
        public void ExtractAll(string where, ZipRestoreFilePathsMode restorePaths = default)
        {
            CheckIfOpened();
            _zip.FlattenFoldersOnExtract = restorePaths == ZipRestoreFilePathsMode.DontRestore;
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
        public void Extract(ZipFileEntryContext entry, string destination, ZipRestoreFilePathsMode restorePaths = default, string password = null)
        {
            CheckIfOpened();
            var realEntry = entry.GetZipEntry();
            _zip.FlattenFoldersOnExtract = restorePaths == ZipRestoreFilePathsMode.DontRestore;
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
        
        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static ZipReader Construct()
        {
            return new ZipReader();
        }

        [ScriptConstructor(Name = "На основании имени файла")]
        public static ZipReader ConstructByNameAndPassword(IValue filename, IValue password = null)
        {
            return new ZipReader(filename.AsString(), password?.AsString());
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
