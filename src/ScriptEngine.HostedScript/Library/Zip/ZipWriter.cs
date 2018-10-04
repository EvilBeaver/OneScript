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
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace ScriptEngine.HostedScript.Library.Zip
{
    /// <summary>
    /// Объект записи ZIP-архивов.
    /// </summary>
    [ContextClass("ЗаписьZipФайла", "ZipFileWriter")]
    public class ZipWriter : AutoContext<ZipWriter>, IDisposable
    {
        
        private ZipOutputStream _zip;

        private string _filename;

        private CompressionMethod compressMeth;

        private EncryptionAlgorithm encryptAlgoritm;
        
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
        [ContextMethod("Открыть", "Open")]
        public void Open(
            string filename, 
            string password = null, 
            string comment = null, 
            SelfAwareEnumValue<ZipCompressionMethodEnum> compressionMethod = null, 
            SelfAwareEnumValue<ZipCompressionLevelEnum> compressionLevel = null,
            SelfAwareEnumValue<ZipEncryptionMethodEnum> encryptionMethod = null)
        {

            _zip = new ZipOutputStream(File.Create(filename));
            
            _filename = filename;
            
            _zip.Password = password;
            _zip.SetComment(comment);
            _zip.SetLevel(MakeZipCompressionLevel(compressionLevel));

            _zip.UseZip64 = UseZip64.Dynamic;
            
            /*
            _zip.AlternateEncoding = Encoding.GetEncoding(866); // fuck non-russian encodings on non-ascii files
            _zip.AlternateEncodingUsage = ZipOption.Always;
             */
               
            compressMeth = MakeZipCompressionMethod(compressionMethod);
            encryptAlgoritm = MakeZipEncryption(encryptionMethod);
              
        }

        /// <summary>
        /// Записывает и закрывает файл архива.
        /// </summary>
        [ContextMethod("Записать", "Write")]
        public void Write()
        {
            CheckIfOpened();

            _zip.Close();

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

            if (pathIsMasked)
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

            var dirInfo = new DirectoryInfo(dir);
            string relativePath;
            if (dir == dirInfo.FullName)
                relativePath = dirInfo.Parent.FullName;
            else
                relativePath = Directory.GetCurrentDirectory();

            AddEnumeratedFiles(filesToAdd, storePathMode, relativePath);
        }

        private void AddSingleFile(string file, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode)
        {
            IEnumerable<string> filesToAdd = new string[] {file};
            AddEnumeratedFiles(filesToAdd, storePathMode, Directory.GetCurrentDirectory());
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
            AddEnumeratedFiles(filesToAdd, storePathMode, Path.GetFullPath(path));

        }

        private void AddEnumeratedFiles(IEnumerable<string> filesToAdd, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode, string relativePath)
        {

            var storeModeEnum = GlobalsManager.GetEnum<ZipStorePathModeEnum>();

            var storeMode = storePathMode;
            if (storeMode == null)
                storeMode = (SelfAwareEnumValue<ZipStorePathModeEnum>)storeModeEnum.StoreRelativePath;

            foreach (var item in filesToAdd)
                AddFileToStream(item, storeMode, relativePath);
        }

        private void AddFileToStream(string fileName, SelfAwareEnumValue<ZipStorePathModeEnum> storeMode, string relativePath)
        {

            var storeModeEnum = GlobalsManager.GetEnum<ZipStorePathModeEnum>();

            var file = new FileInfo(fileName);

            string pathInArchive = null;
            if (storeMode == storeModeEnum.StoreFullPath)
                pathInArchive = file.FullName;
            else if (storeMode == storeModeEnum.StoreRelativePath)
            {
                pathInArchive = file.FullName;
                if (pathInArchive == relativePath)
                    pathInArchive = string.Empty;
                else if (pathInArchive.StartsWith(relativePath))
                    pathInArchive = pathInArchive.Substring(relativePath.Length);
                else
                    pathInArchive = file.Name;
            }
            else if (storeMode == storeModeEnum.DontStorePath)
                pathInArchive = file.Name;

            var entry = new ZipEntry(pathInArchive);
            entry.DateTime = DateTime.Now;
            entry.CompressionMethod = compressMeth;
            
            _zip.PutNextEntry(entry);

            using (FileStream fs = File.OpenRead(fileName))
                fs.CopyTo(_zip);

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
                return CompressionMethod.Deflated;

            var owner = (ZipCompressionMethodEnum)compressionMethod.Owner;
            if (compressionMethod == owner.Deflate)
                return CompressionMethod.Deflated;
            if (compressionMethod == owner.Copy)
                return CompressionMethod.Stored;

            throw RuntimeException.InvalidArgumentValue();

        }

        private int MakeZipCompressionLevel(SelfAwareEnumValue<ZipCompressionLevelEnum> compressionLevel)
        {
            if (compressionLevel == null)
                return Deflater.DEFAULT_COMPRESSION;

            var owner = (ZipCompressionLevelEnum)compressionLevel.Owner;
            if (compressionLevel == owner.Minimal)
                return Deflater.BEST_SPEED;
            if (compressionLevel == owner.Optimal)
                return Deflater.DEFAULT_COMPRESSION;
            if (compressionLevel == owner.Maximal)
                return Deflater.BEST_COMPRESSION;

            throw RuntimeException.InvalidArgumentValue();
        }

        private EncryptionAlgorithm MakeZipEncryption(SelfAwareEnumValue<ZipEncryptionMethodEnum> encryptionMethod)
        {
            if (encryptionMethod == null)
                return EncryptionAlgorithm.PkzipClassic;
            
            var enumOwner = (ZipEncryptionMethodEnum)encryptionMethod.Owner;

            if(encryptionMethod == enumOwner.Zip20)
                return EncryptionAlgorithm.PkzipClassic;
            if (encryptionMethod == enumOwner.Aes128)
                return EncryptionAlgorithm.Aes128;
            if (encryptionMethod == enumOwner.Aes256)
                return EncryptionAlgorithm.Aes256;

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
        public static ZipWriter ConstructByFileOptions(IValue filename, IValue password = null, IValue comment = null, IValue compressionMethod = null, IValue compressionLevel = null, IValue encryptionMethod = null)
        {
            var zip = new ZipWriter();
            zip.Open(filename.AsString(),
                ConvertParam<string>(password),
                ConvertParam<string>(comment),
                ConvertParam<SelfAwareEnumValue<ZipCompressionMethodEnum>>(compressionMethod),
                ConvertParam<SelfAwareEnumValue<ZipCompressionLevelEnum>>(compressionLevel),
                ConvertParam<SelfAwareEnumValue<ZipEncryptionMethodEnum>>(encryptionMethod)
                    );
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
                    _zip.Close();
                    _zip.Dispose();
                    _zip = null;
                }
            }
        }

        #endregion
    }
}
