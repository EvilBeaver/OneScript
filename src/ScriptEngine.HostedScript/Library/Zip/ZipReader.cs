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

using ICSharpCode.SharpZipLib.Zip;

namespace ScriptEngine.HostedScript.Library.Zip
{
    /// <summary>
    /// Объект чтения ZIP файлов.
    /// </summary>
    [ContextClass("ЧтениеZipФайла", "ZipFileReader")]
    public class ZipReader : AutoContext<ZipReader>, IDisposable
    {
        private ZipFile _zip;
        private ZipFileEntriesCollection _entriesWrapper;

        private string _password;

        public ZipReader()
        {
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
        [ContextMethod("Открыть","Open")]
        public void Open(string filename, string password = null)
        {

            _zip = new ZipFile(filename);
            _zip.Password = password;
            _password = password;

        }


        /// <summary>
        /// Извлечение всех файлов из архива
        /// </summary>
        /// <param name="where">Строка. Каталог в который извлекаются файлы</param>
        /// <param name="restorePaths">РежимВосстановленияПутейФайловZIP</param>
        [ContextMethod("ИзвлечьВсе","ExtractAll")]
        public void ExtractAll(string destination, SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths = null)
        {
            CheckIfOpened();

            foreach (var entry in Elements)
            {
                Extract(entry, destination, restorePaths);
            }

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
            var restoreFoldersOnExtract = GetRestorePathsOnExtractionFlag(restorePaths);

            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            string fileName;
            if (realEntry.IsDirectory)
            {
                if (restoreFoldersOnExtract)
                    Directory.CreateDirectory(Path.Combine(destination, entry.Path, realEntry.Name));
            }
            else
            {

                if (restoreFoldersOnExtract)
                {
                    fileName = Path.Combine(destination, entry.Path, entry.Name);
                    var fileInfo = new FileInfo(fileName);
                    if (!Directory.Exists(fileInfo.DirectoryName))
                        Directory.CreateDirectory(fileInfo.DirectoryName);
                }
                else
                    fileName = Path.Combine(destination, entry.Name);


                var fileMode = (File.Exists(fileName)) ? FileMode.Create : FileMode.CreateNew;

                if (password != null)
                    _zip.Password = password;

                try
                {
                    using (var streamReader = _zip.GetInputStream(realEntry.ZipFileIndex))
                        using (var streamWriter = new FileStream(fileName, fileMode))
                            streamReader.CopyTo(streamWriter);
                }
                finally
                {
                    if (password != null)
                        _zip.Password = _password;
                }
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
                    _entriesWrapper = new ZipFileEntriesCollection(GetEntries());

                return _entriesWrapper;
            }
        }

        private IEnumerable<ZipEntry> GetEntries()
        {
            var enumer = _zip.GetEnumerator();
            while (enumer.MoveNext())
                yield return (ZipEntry)enumer.Current;
        }


        private static bool GetRestorePathsOnExtractionFlag(SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths)
        {
            bool restoreFlag = true; // default
            if (restorePaths != null)
            {
                var zipEnum = (ZipRestoreFilePathsModeEnum)restorePaths.Owner;
                restoreFlag = restorePaths == zipEnum.Restore;
            }

            return restoreFlag;
        }

        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static ZipReader Construct()
        {
            return new ZipReader();
        }

        [ScriptConstructor(Name = "На основании имени файла")]
        public static ZipReader ConstructByNameAndPassword(IValue filename, IValue password = null)
        {
            var zipReader = new ZipReader();
            zipReader.Open(filename.AsString(), password?.AsString());

            return zipReader;
        }

        public void Dispose()
        {
            _entriesWrapper = null;
            if (_zip != null)
            {
                _zip.Close();
                _zip = null;
            }
        }
    }
}
