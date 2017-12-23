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
using System.Linq;
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
        [ContextMethod("Открыть", "Open")]
        public void Open(
            string filename, 
            string password = null, 
            string comment = null, 
            SelfAwareEnumValue<ZipCompressionMethodEnum> compressionMethod = null, 
            SelfAwareEnumValue<ZipCompressionLevelEnum> compressionLevel = null,
            SelfAwareEnumValue<ZipEncryptionMethodEnum> encryptionMethod = null)
        {
            _filename = filename;
            _zip = new ZipFile();
            _zip.AlternateEncoding = Encoding.GetEncoding(866); // fuck non-russian encodings on non-ascii files
            _zip.AlternateEncodingUsage = ZipOption.Always;
            _zip.Password = password;
            _zip.Comment = comment;
            _zip.CompressionMethod = MakeZipCompressionMethod(compressionMethod);
            _zip.CompressionLevel = MakeZipCompressionLevel(compressionLevel);
            _zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
            // Zlib падает с NullReferenceException, если задать шифрование
            //_zip.Encryption = MakeZipEncryption(encryptionMethod);
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
            var searchOption = recursiveFlag ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;

            if(pathIsMasked)
            {
                AddFilesByMask(file, searchOption, storePathMode);
            }
            else if (System.IO.Directory.Exists(file))
            {
                AddDirectory(file, searchOption, storePathMode);
            }
            else if (System.IO.File.Exists(file))
            {
                AddSingleFile(file, storePathMode);
            }
            
        }

        private void AddDirectory(string file, System.IO.SearchOption searchOption, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode)
        {
            string allFilesMask;

            if (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX)
                allFilesMask = "*";
            else
                allFilesMask = "*.*";

            var filesToAdd = System.IO.Directory.EnumerateFiles(file, allFilesMask, searchOption);
            AddEnumeratedFiles(filesToAdd, GetPathForParentFolder(file), storePathMode);
        }

        private string GetPathForParentFolder(string dirpath)
        {
            DebugEcho(String.Format("GetPathForParentFolder dirpath is {0}", dirpath));
            var pathForParentFolder = "";
            if (Path.IsPathRooted(dirpath))
            {
                DebugEcho("GetPathForParentFolder: is rooted");

                var currDir = System.IO.Directory.GetCurrentDirectory();
                DebugEcho(String.Format("GetPathForParentFolder currDir is {0}", currDir));

                var path = GetRelativePath(dirpath, currDir);
                DebugEcho(String.Format("GetPathForParentFolder GetRelativePath is {0}", path));
                if (IsNotRelativePath(dirpath, path))
                {
                    DebugEcho("GetPathForParentFolder IsNotRelativePath is true");
                    pathForParentFolder = System.IO.Path.Combine(dirpath, "..");
                }
                else
                {
                    DebugEcho("GetPathForParentFolder IsNotRelativePath is false");
                    pathForParentFolder = currDir;
                }
            }
            else
            {
                DebugEcho("GetPathForParentFolder: is not rooted");
                pathForParentFolder = System.IO.Path.Combine(dirpath, "..");
            }

            DebugEcho(String.Format("GetPathForParentFolder pathForParentFolder is {0}", pathForParentFolder));
            return pathForParentFolder;
        }

        private bool IsNotRelativePath(string filepath, string relativePath)
        {
            DebugEcho(String.Format("IsNotRelativePath: filepath is {0}, relativePath is {1}", filepath, relativePath));
            return (relativePath == filepath || relativePath.Substring(0, 2) == "..");
        }

        private void AddSingleFile(string file, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode)
        {
            var storeModeEnum = GlobalsManager.GetEnum<ZipStorePathModeEnum>();
            if (storePathMode == null)
                storePathMode = (SelfAwareEnumValue<ZipStorePathModeEnum>)storeModeEnum.StoreRelativePath;

            var currDir = System.IO.Directory.GetCurrentDirectory();

            string pathInArchive;
            if (storePathMode == storeModeEnum.StoreFullPath)
                pathInArchive = null;
            else if (storePathMode == storeModeEnum.StoreRelativePath)
            {
                DebugEcho(String.Format("AddSingleFile: file is {0}", file));
                var relativePath = GetRelativePath(file, currDir);
                DebugEcho(String.Format("AddSingleFile: relativePath is {0}", relativePath));
                if (Path.IsPathRooted(file) && IsNotRelativePath(file, relativePath))
                {
                    DebugEcho("Path.IsPathRooted(file) && IsNotRelativePath(file, relativePath)");
                    pathInArchive = ".";
                }
                else
                {
                    DebugEcho("NOT Path.IsPathRooted(file) && IsNotRelativePath(file, relativePath)");
                    pathInArchive = Path.GetDirectoryName(relativePath);
                }
            }
            else
                pathInArchive = "";

            DebugEcho(String.Format("AddSingleFile: pathInArchive is {0}", pathInArchive));
            _zip.AddFile(file, pathInArchive);
        }

        private void AddFilesByMask(string file, System.IO.SearchOption searchOption, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode)
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
                if (!System.IO.Directory.Exists(path))
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

            filesToAdd = System.IO.Directory.EnumerateFiles(path, mask, searchOption);
            var relativePath = System.IO.Path.GetFullPath(path);
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
                {
                    DebugEcho(String.Format("AddEnumeratedFiles: relativePath is {0}", relativePath));
                    DebugEcho(String.Format("AddEnumeratedFiles: item is {0}", item));
                    DebugEcho(String.Format("AddEnumeratedFiles: GetRelativePath(item, relativePath) is {0}", GetRelativePath(item, relativePath)));
                    pathInArchive = System.IO.Path.GetDirectoryName(GetRelativePath(item, relativePath));
                }
                else if (storePathMode == storeModeEnum.StoreFullPath)
                    pathInArchive = null;
                else
                    pathInArchive = "";

                DebugEcho(String.Format("AddEnumeratedFiles: pathInArchive is {0}", pathInArchive));
                _zip.AddFile(item, pathInArchive);
            }
        }

        private string GetRelativePath(string filespec, string folder)
        {
            DebugEcho(String.Format("GetRelativePath: filespec is {0}, folder is {1}", filespec, folder));
            Uri pathUri = null;
            try
            {
                pathUri = new Uri("file://" + filespec);
            }
            catch (System.UriFormatException)
            {
                DebugEcho(String.Format("GetRelativePath catch is {0}", filespec));
                return filespec;
            }

            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri("file://" + folder);
            var res = Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
            DebugEcho(String.Format("GetRelativePath res is {0}", res));

            var removestr = "file:\\\\";
            if (res.StartsWith(removestr))
            {
                DebugEcho(String.Format("GetRelativePath res without {1} is {0}", res, removestr));
                res = res.Substring(removestr.Length);
            }
            removestr = "file://";
            if (res.StartsWith(removestr))
            {
                DebugEcho(String.Format("GetRelativePath res without {1} is {0}", res, removestr));
                res = res.Substring(removestr.Length);
            }
            return res;
        }

        private void DebugEcho(string str)
        {
            Console.WriteLine(String.Format("DEBUG ZIP: {0}", str));
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
                    _zip.Dispose();
                    _zip = null;
                }
            }
        }

        #endregion
    }
}
