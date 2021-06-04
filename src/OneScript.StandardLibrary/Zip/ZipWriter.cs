/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Zip;
using Ionic.Zlib;
using OneScript.Commons;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Zip
{
    /// <summary>
    /// Объект записи ZIP-архивов.
    /// </summary>
    [ContextClass("ЗаписьZipФайла", "ZipFileWriter")]
    public class ZipWriter : AutoContext<ZipWriter>, IDisposable
    {
        private ZipFile _zip;
        private string _filename;

        public ZipWriter()
        {

        }

        /// <summary>
        /// Открыть архив для записи.
        /// </summary>
        /// <param name="filename">Имя файла будущего архива</param>
        /// <param name="password">Пароль на архив</param>
        /// <param name="comment">Комментарий к архиву</param>
        /// <param name="compressionMethod">МетодСжатияZIP (Сжатие/Копирование)</param>
        /// <param name="compressionLevel">УровеньСжатияZIP (Минимальный/Оптимальный/Максимальный)</param>
        /// <param name="encryptionMethod">МетодШифрованияZIP (в текущей реализации не поддерживается)</param>
        /// <param name="encoding">Кодировка имен файлов в архиве.</param>
        [ContextMethod("Открыть", "Open")]
        public void Open(
            string filename, 
            string password = null, 
            string comment = null, 
            ZipCompressionMethod compressionMethod = default, 
            ZipCompressionLevel compressionLevel = default,
            ZipEncryptionMethod? encryptionMethod = default,
            FileNamesEncodingInZipFile encoding = FileNamesEncodingInZipFile.Auto)
        {
            ZipFile.DefaultEncoding = Encoding.GetEncoding(866); // fuck non-russian encodings on non-ascii files
            _filename = filename;
            _zip = new ZipFile();
            _zip.AlternateEncoding = Encoding.UTF8;
            _zip.AlternateEncodingUsage = ChooseEncodingMode(encoding);
            _zip.Password = password;
            _zip.Comment = comment;
            _zip.CompressionMethod = MakeZipCompressionMethod(compressionMethod);
            _zip.CompressionLevel = MakeZipCompressionLevel(compressionLevel);
            _zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
            
            // Zlib падает с NullReferenceException, если задать шифрование
            //_zip.Encryption = MakeZipEncryption(encryptionMethod);
        }

        private ZipOption ChooseEncodingMode(FileNamesEncodingInZipFile encoding)
        {
            if (encoding == FileNamesEncodingInZipFile.OsEncodingWithUtf8)
                return ZipOption.AsNecessary;
            
            return ZipOption.Always;
        }

        /// <summary>
        /// Записывает и закрывает файл архива.
        /// </summary>
        [ContextMethod("Записать", "Write")]
        public void Write()
        {
            CheckIfOpened();

            _zip.Save(_filename);
            Dispose(true);
        }

        /// <summary>
        /// Добавление файла к архиву.
        /// </summary>
        /// <param name="file">Имя файла, помещаемого в архив, или маска.</param>
        /// <param name="storePathMode">РежимСохраненияПутейZIP (НеСохранятьПути/СохранятьОтносительныеПути/СохранятьПолныеПути)</param>
        /// <param name="recurseSubdirectories">РежимОбработкиПодкаталоговZIP (НеОбрабатывать/ОбрабатыватьРекурсивно)</param>
        [ContextMethod("Добавить", "Add")]
        public void Add(string file, ZipStorePathMode? storePathMode = default, ZipSubDirProcessingMode recurseSubdirectories = default)
        {
            CheckIfOpened();

            var pathIsMasked = file.IndexOfAny(new[] { '*', '?' }) >= 0;

            var recursiveFlag = recurseSubdirectories != ZipSubDirProcessingMode.DontRecurse;
            var searchOption = recursiveFlag ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            if(pathIsMasked)
            {
                AddFilesByMask(file, searchOption, storePathMode);
            }
            else if (Directory.Exists(file))
            {
                AddDirectory(file, searchOption, storePathMode);
            }
            else if (File.Exists(file))
            {
                AddSingleFile(file, storePathMode);
            }
            
        }

        private void AddDirectory(string dir, SearchOption searchOption, ZipStorePathMode? storePathMode)
        {
            string allFilesMask;

            if (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX)
                allFilesMask = "*";
            else
                allFilesMask = "*.*";

            var filesToAdd = Directory.EnumerateFiles(dir, allFilesMask, searchOption);
            AddEnumeratedFiles(filesToAdd, GetPathForParentFolder(dir), storePathMode);
        }

        private string GetPathForParentFolder(string dir)
        {
            var rootPath = GetRelativePath(dir, Directory.GetCurrentDirectory());
            if (rootPath == "")
                rootPath = Path.Combine(Directory.GetCurrentDirectory(), dir, "..");
            else
                rootPath = Directory.GetCurrentDirectory();
            return rootPath;
        }

        private void AddSingleFile(string file, ZipStorePathMode? storePathMode)
        {
            if (storePathMode == null)
                storePathMode = ZipStorePathMode.StoreRelativePath;

            var currDir = Directory.GetCurrentDirectory();

            string pathInArchive;
            if (storePathMode == ZipStorePathMode.StoreFullPath)
                pathInArchive = null;
            else if (storePathMode == ZipStorePathMode.StoreRelativePath)
            {
                var relativePath = GetRelativePath(file, currDir);
                if (relativePath == "")
                    pathInArchive = ".";
                else
                    pathInArchive = Path.GetDirectoryName(relativePath);
            }
            else
                pathInArchive = "";

            _zip.AddFile(file, pathInArchive);
        }

        private void AddFilesByMask(string file, SearchOption searchOption, ZipStorePathMode? storePathMode)
        {
            // надо разделить на каталог и маску
            var pathEnd = file.LastIndexOfAny(new[] { '\\', '/' });
            string path;
            string mask;
            IEnumerable<string> filesToAdd;

            if (pathEnd > 1)
            {
                path = file.Substring(0, pathEnd);
                var maskLen = file.Length - pathEnd - 1;
                if (maskLen > 0)
                    mask = file.Substring(pathEnd + 1, maskLen);
                else
                {
                    // маска была не в конце пути
                    // 1С такое откидывает
                    return;
                }

                // несуществующие пути или пути к файлам, вместо папок 1С откидывает
                if (!Directory.Exists(path))
                    return;
            }
            else if (pathEnd == 0)
            {
                path = "";
                mask = file.Substring(1);
            }
            else
            {
                path = "";
                mask = file;
            }

            filesToAdd = Directory.EnumerateFiles(path, mask, searchOption);
            var relativePath = Path.GetFullPath(path);
            AddEnumeratedFiles(filesToAdd, relativePath, storePathMode);

        }

        private void AddEnumeratedFiles(IEnumerable<string> filesToAdd, string relativePath, ZipStorePathMode? storePathMode)
        {
            if (storePathMode == null)
                storePathMode = ZipStorePathMode.DontStorePath;

            foreach (var item in filesToAdd)
            {
                string pathInArchive;
                if (storePathMode == ZipStorePathMode.StoreRelativePath)
                    pathInArchive = Path.GetDirectoryName(GetRelativePath(item, relativePath));
                else if (storePathMode == ZipStorePathMode.StoreFullPath)
                    pathInArchive = null;
                else
                    pathInArchive = "";

                _zip.AddFile(item, pathInArchive);
            }
        }

        // возвращает относительный путь или "", если путь не является относительным
        private string GetRelativePath(string filespec, string rootfolder)
        {
            var currDir = Directory.GetCurrentDirectory();

            DirectoryInfo directory = new DirectoryInfo(Path.Combine(currDir, rootfolder));
            var folderpath = directory.FullName;

            var filepath = Path.Combine(currDir, filespec);

            if (Directory.Exists(filespec))
            {
                DirectoryInfo dir = new DirectoryInfo(filepath);
                filepath = dir.FullName;
            }
            else {
                FileInfo file = new FileInfo(filepath);
                filepath = file.FullName;
            }

            if (!filepath.StartsWith(folderpath))
                return "";

            var res = filepath.Substring(folderpath.Length + 1);
            if (res == "")
                res = ".";
            return res;
        }
        
        private CompressionMethod MakeZipCompressionMethod(ZipCompressionMethod compressionMethod)
        {
            if (compressionMethod == ZipCompressionMethod.Deflate)
                return CompressionMethod.Deflate;
            if (compressionMethod == ZipCompressionMethod.Copy)
                return CompressionMethod.None;

            throw RuntimeException.InvalidArgumentValue();

        }

        private CompressionLevel MakeZipCompressionLevel(ZipCompressionLevel compressionLevel)
        {
            if (compressionLevel == default)
                return CompressionLevel.Default;

            if (compressionLevel == ZipCompressionLevel.Minimal)
                return CompressionLevel.BestSpeed;
            if (compressionLevel == ZipCompressionLevel.Optimal)
                return CompressionLevel.Default;
            if (compressionLevel == ZipCompressionLevel.Maximal)
                return CompressionLevel.BestCompression;

            throw RuntimeException.InvalidArgumentValue();
        }

        private EncryptionAlgorithm MakeZipEncryption(ZipEncryptionMethod? encryptionMethod)
        {
            if (encryptionMethod == null)
                return EncryptionAlgorithm.PkzipWeak;
            
            if(encryptionMethod == ZipEncryptionMethod.Zip20)
                return EncryptionAlgorithm.PkzipWeak;
            if (encryptionMethod == ZipEncryptionMethod.Aes128)
                return EncryptionAlgorithm.WinZipAes128;
            if (encryptionMethod == ZipEncryptionMethod.Aes256)
                return EncryptionAlgorithm.WinZipAes256;

            throw RuntimeException.InvalidArgumentValue();

        }

        private void CheckIfOpened()
        {
            if (_zip == null)
                throw new RuntimeException("Архив не открыт");
        }

        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static ZipWriter Construct()
        {
            return new ZipWriter();
        }

        [ScriptConstructor(Name = "На основании имени файла")]
        public static ZipWriter ConstructByFileOptions(
            IValue filename, 
            IValue password = null,
            IValue comment = null,
            IValue compressionMethod = null,
            IValue compressionLevel = null,
            IValue encryptionMethod = null,
            FileNamesEncodingInZipFile encoding = FileNamesEncodingInZipFile.Auto)
        {
            var zip = new ZipWriter();
            zip.Open(filename.AsString(),
                ConvertParam<string>(password),
                ConvertParam<string>(comment),
                ConvertParam<ZipCompressionMethod>(compressionMethod),
                ConvertParam<ZipCompressionLevel>(compressionLevel),
                ConvertParam<ZipEncryptionMethod>(encryptionMethod),
                    encoding);
            return zip;
        }

        private static T ConvertParam<T>(IValue paramSource)
        {
            if (paramSource == null)
                return default(T);

            var raw = paramSource.GetRawValue();
            if (raw.IsSkippedArgument())
                return default(T);

            if (typeof(EnumerationValue).IsAssignableFrom(typeof(T)))
            {
                try
                {
                    return (T)raw;
                }
                catch (InvalidCastException)
                {
                    throw RuntimeException.InvalidArgumentType();
                }
            }
            else
                return ContextValuesMarshaller.ConvertParam<T>(raw);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_zip != null)
                {
                    _zip.Dispose();
                    _zip = null;
                }
            }
        }

        #endregion
    }
}
