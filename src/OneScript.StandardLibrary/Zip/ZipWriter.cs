/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Ionic.Zip;
using Ionic.Zlib;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Zip
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
            SelfAwareEnumValue<ZipCompressionMethodEnum> compressionMethod = null, 
            SelfAwareEnumValue<ZipCompressionLevelEnum> compressionLevel = null,
            SelfAwareEnumValue<ZipEncryptionMethodEnum> encryptionMethod = null,
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
        public void Add(string file, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode = null, SelfAwareEnumValue<ZIPSubDirProcessingModeEnum> recurseSubdirectories = null)
        {
            CheckIfOpened();

            var pathIsMasked = file.IndexOfAny(new[] { '*', '?' }) >= 0;

            var recursiveFlag = GetRecursiveFlag(recurseSubdirectories);
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

        private void AddDirectory(string dir, SearchOption searchOption, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode)
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

        private void AddSingleFile(string file, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode)
        {
            var storeModeEnum = GlobalsManager.GetEnum<ZipStorePathModeEnum>();
            if (storePathMode == null)
                storePathMode = (SelfAwareEnumValue<ZipStorePathModeEnum>)storeModeEnum.StoreRelativePath;

            var currDir = Directory.GetCurrentDirectory();

            string pathInArchive;
            if (storePathMode == storeModeEnum.StoreFullPath)
                pathInArchive = null;
            else if (storePathMode == storeModeEnum.StoreRelativePath)
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

        private void AddFilesByMask(string file, SearchOption searchOption, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode)
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

        private void AddEnumeratedFiles(IEnumerable<string> filesToAdd, string relativePath, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode)
        {
            var storeModeEnum = GlobalsManager.GetEnum<ZipStorePathModeEnum>();
            if (storePathMode == null)
                storePathMode = (SelfAwareEnumValue<ZipStorePathModeEnum>)storeModeEnum.DontStorePath;

            foreach (var item in filesToAdd)
            {
                string pathInArchive;
                if (storePathMode == storeModeEnum.StoreRelativePath)
                    pathInArchive = Path.GetDirectoryName(GetRelativePath(item, relativePath));
                else if (storePathMode == storeModeEnum.StoreFullPath)
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

        private static bool GetRecursiveFlag(SelfAwareEnumValue<ZIPSubDirProcessingModeEnum> recurseSubdirectories)
        {
            if (recurseSubdirectories == null)
                return false;
            else
                return recurseSubdirectories == ((ZIPSubDirProcessingModeEnum)recurseSubdirectories.Owner).Recurse;
        }

        private CompressionMethod MakeZipCompressionMethod(SelfAwareEnumValue<ZipCompressionMethodEnum> compressionMethod)
        {
            if (compressionMethod == null)
                return CompressionMethod.Deflate;

            var owner = (ZipCompressionMethodEnum)compressionMethod.Owner;
            if (compressionMethod == owner.Deflate)
                return CompressionMethod.Deflate;
            if (compressionMethod == owner.Copy)
                return CompressionMethod.None;

            throw RuntimeException.InvalidArgumentValue();

        }

        private CompressionLevel MakeZipCompressionLevel(SelfAwareEnumValue<ZipCompressionLevelEnum> compressionLevel)
        {
            if (compressionLevel == null)
                return CompressionLevel.Default;

            var owner = (ZipCompressionLevelEnum)compressionLevel.Owner;
            if (compressionLevel == owner.Minimal)
                return CompressionLevel.BestSpeed;
            if (compressionLevel == owner.Optimal)
                return CompressionLevel.Default;
            if (compressionLevel == owner.Maximal)
                return CompressionLevel.BestCompression;

            throw RuntimeException.InvalidArgumentValue();
        }

        private EncryptionAlgorithm MakeZipEncryption(SelfAwareEnumValue<ZipEncryptionMethodEnum> encryptionMethod)
        {
            if (encryptionMethod == null)
                return EncryptionAlgorithm.PkzipWeak;
            
            var enumOwner = (ZipEncryptionMethodEnum)encryptionMethod.Owner;

            if(encryptionMethod == enumOwner.Zip20)
                return EncryptionAlgorithm.PkzipWeak;
            if (encryptionMethod == enumOwner.Aes128)
                return EncryptionAlgorithm.WinZipAes128;
            if (encryptionMethod == enumOwner.Aes256)
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
                ConvertParam<SelfAwareEnumValue<ZipCompressionMethodEnum>>(compressionMethod),
                ConvertParam<SelfAwareEnumValue<ZipCompressionLevelEnum>>(compressionLevel),
                ConvertParam<SelfAwareEnumValue<ZipEncryptionMethodEnum>>(encryptionMethod),
                    encoding);
            return zip;
        }

        private static T ConvertParam<T>(IValue paramSource)
        {
            if (paramSource == null)
                return default(T);

            if (paramSource.DataType == DataType.NotAValidValue)
                return default(T);

            var raw = paramSource.GetRawValue();
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
