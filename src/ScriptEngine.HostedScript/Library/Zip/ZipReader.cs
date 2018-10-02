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
        ZipFile _zip;
        ZipFileEntriesCollection _entriesWrapper;
        string _password;

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
        /// <param name="overwrite">Признак замены существующих файлов при распаковке</param>
        [ContextMethod("ИзвлечьВсе","ExtractAll")]
        public void ExtractAll(string destination, SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths = null, bool overwrite = true)
        {
            CheckIfOpened();

            foreach (var entry in Elements)
            {
                Extract(entry, destination, restorePaths, null, overwrite);
            }

        }

        /// <summary>
        /// Извлечение элемента из архива
        /// </summary>
        /// <param name="entry">ЭлементZipФайла. Извлекаемый элемент.</param>
        /// <param name="destination">Каталог, в который извлекается элемент.</param>
        /// <param name="restorePaths">РежимВосстановленияПутейФайлов</param>
        /// <param name="password">Пароль элемента (если отличается от пароля к архиву)</param>
        /// <param name="overwrite">Признак замены существующего файла при распаковке</param>
        [ContextMethod("Извлечь", "Extract")]
        public void Extract(ZipFileEntryContext entry, string destination, SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths = null, string password = null, bool overwrite = true)
        {
            CheckIfOpened();

            var realEntry = entry.GetZipEntry();
            var flattenFoldersOnExtract = FlattenPathsOnExtraction(restorePaths);

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            string fileName;
            if (realEntry.IsDirectory)
            {
                if (flattenFoldersOnExtract)
                    Directory.CreateDirectory(Path.Combine(destination, entry.Path, realEntry.Name));
            }
            else
            {
                if (flattenFoldersOnExtract)
                    fileName = Path.Combine(destination, entry.Path, entry.Name);
                else
                    fileName = Path.Combine(destination, entry.Name);

                if (password != null)
                    _zip.Password = password;

                var fileMode = (overwrite) ? FileMode.Create : FileMode.CreateNew;

                using (var streamReader = _zip.GetInputStream(realEntry.ZipFileIndex))
                    using (var streamWriter = new FileStream(fileName, fileMode))
                        streamReader.CopyTo(streamWriter);

                if (password != null)
                    _zip.Password = _password;
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
