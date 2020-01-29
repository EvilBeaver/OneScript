/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using OneScript.StandardLibrary.Collections;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
{
    [GlobalContext(Category="Файловые операции")]
    public class FileOperations : GlobalContextBase<FileOperations>
    {

        /// <summary>
        /// Копирует файл из одного расположения в другое. Перезаписывает приемник, если он существует.
        /// </summary>
        /// <param name="source">Имя файла-источника</param>
        /// <param name="destination">Имя файла приемника</param>
        [ContextMethod("КопироватьФайл", "CopyFile")]
        public void CopyFile(string source, string destination)
        {
            var scheme = PathScheme(source);

            if(scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeHttps)
                DownloadFromRemote<HttpWebRequest>(source, 
                    destination, WebRequestMethods.Http.Get);
            else if(scheme == Uri.UriSchemeFtp)
                DownloadFromRemote<FtpWebRequest>(source, 
                    destination, WebRequestMethods.Ftp.DownloadFile);
            else
                File.Copy(source, destination, true);

        }

        /// <summary>
        /// Перемещает файл из одного расположения в другое.
        /// </summary>
        /// <param name="source">Имя файла-источника</param>
        /// <param name="destination">Имя файла приемника</param>
        [ContextMethod("ПереместитьФайл", "MoveFile")]
        public void MoveFile(string source, string destination)
        {
            var scheme = PathScheme(source);

            if (scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeHttps)
            {
                DownloadFromRemote<HttpWebRequest>(source,
                    destination, WebRequestMethods.Http.Get);
                DeleteFromRemote<HttpWebRequest>(source, "DELETE");
            }
            else if (scheme == Uri.UriSchemeFtp)
            {
                DownloadFromRemote<FtpWebRequest>(source,
                    destination, WebRequestMethods.Ftp.DownloadFile);
                DeleteFromRemote<FtpWebRequest>(source, WebRequestMethods.Ftp.DeleteFile);
            }
            else
                File.Move(source, destination);
        }

        public string PathScheme(string path)
        {
            if(Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri uri) && uri.IsAbsoluteUri)
            {
                return uri.Scheme;
            }
            return Uri.UriSchemeFile;
        }

        private void DownloadFromRemote<T>(string source,
            string destination, string method) where T: WebRequest
        {
            var req = (T)WebRequest.Create(source);
            req.Method = method;

            using (var respStream = req.GetResponse().GetResponseStream())
                using (var fs = File.Create(destination))
                    respStream.CopyTo(fs);
        }

        private void DeleteFromRemote<T>(string source, string method) where T : WebRequest
        {
            var req = (T)WebRequest.Create(source);
            req.Method = method;

            using (var resp = req.GetResponse()) { };
        }

        /// <summary>
        /// Возвращает каталог временных файлов ОС
        /// </summary>
        /// <returns>Строка. Путь к каталогу временных файлов</returns>
        [ContextMethod("КаталогВременныхФайлов", "TempFilesDir")]
        public string TempFilesDir()
        {
            return Path.GetTempPath();
        }

        /// <summary>
        /// Получает имя файла во временом каталоге.
        /// </summary>
        /// <param name="ext">Расширение будущего файла. Если не указано, то по умолчанию расширение равно ".tmp"</param>
        /// <returns>Строка. Полный путь ко временному файлу.</returns>
        [ContextMethod("ПолучитьИмяВременногоФайла", "GetTempFileName")]
        public string GetTempFilename(string ext = null)
        {
            // примитивная реализация "в лоб"
            var fn = Path.GetRandomFileName();
            if (ext != null && !String.IsNullOrWhiteSpace(ext))
            {
                if(ext[0] == '.')
                    fn += ext;
                else
                    fn += "." + ext;
            }

            return Path.Combine(TempFilesDir(), fn);

        }

        /// <summary>
        /// Выполняет поиск файлов по маске
        /// </summary>
        /// <param name="dir">Каталог, в котором выполняется поиск</param>
        /// <param name="mask">Маска имени файла (включая символы * и ?)</param>
        /// <param name="recursive">Флаг рекурсивного поиска в поддиректориях</param>
        /// <returns>Массив объектов Файл, которые были найдены.</returns>
        [ContextMethod("НайтиФайлы", "FindFiles")]
        public ArrayImpl FindFiles(string dir, string mask = null, bool recursive = false)
        {
            if (mask == null)
            {
                // fix 225, 227, 228
                var fObj = new FileContext(dir);
                if(fObj.Exist())
                {
                    return new ArrayImpl(new[] { fObj });
                }
                else
                {
                    return new ArrayImpl();
                }
            }
            else if (File.Exists(dir))
            {
                return new ArrayImpl();
            }

            if(!Directory.Exists(dir))
                return new ArrayImpl();

            var filesFound = FindFilesV8Compatible(dir, mask, recursive);

            return new ArrayImpl(filesFound);

        }

        private static IEnumerable<FileContext> FindFilesV8Compatible(string dir, string mask, bool recursive)
        {
            var collectedFiles = new List<FileContext>();
            IEnumerable<FileContext> entries;
            IEnumerable<FileContext> folders = null;
            try
            {
                if (recursive)
                    folders = Directory.GetDirectories(dir).Select(x => new FileContext(x));

                entries = Directory.EnumerateFileSystemEntries(dir, mask)
                                   .Select(x => new FileContext(x));
            }
            catch (SecurityException)
            {
                return collectedFiles;
            }
            catch (UnauthorizedAccessException)
            {
                return collectedFiles;
            }

            if (recursive)
            {
                foreach (var fileFound in entries)
                {
                    try
                    {
                        var attrs = fileFound.GetAttributes();
                        if (attrs.HasFlag(FileAttributes.ReparsePoint))
                        {
                            collectedFiles.Add(fileFound);
                            continue;
                        }
                    }
                    catch (SecurityException)
                    {
                        continue;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }

                    collectedFiles.Add(fileFound);
                }

                foreach (var folder in folders)
                {
                    try
                    {
                        var attrs = folder.GetAttributes();
                        if (!attrs.HasFlag(FileAttributes.ReparsePoint))
                        {
                            collectedFiles.AddRange(FindFilesV8Compatible(folder.FullName, mask, true));
                        }
                    }
                    catch (SecurityException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            }
            else
            {
                collectedFiles.AddRange(entries);
            }

            return collectedFiles;
        }

        /// <summary>
        /// Удаление файлов
        /// </summary>
        /// <param name="path">Каталог из которого удаляются файлы, или сам файл.</param>
        /// <param name="mask">Маска файлов. Необязательный параметр. Если указан, то первый параметр трактуется, как каталог.</param>
        [ContextMethod("УдалитьФайлы", "DeleteFiles")]
        public void DeleteFiles(string path, string mask = null)
        {
            if (mask == null)
            {
                if (Directory.Exists(path))
                {
                    System.IO.Directory.Delete(path, true);
                }
                else
                {
                    System.IO.File.Delete(path);
                }
            }
            else
            {
                // bugfix #419
                if (!Directory.Exists(path))
                    return;

                var entries = System.IO.Directory.EnumerateFileSystemEntries(path, mask)
                    .AsParallel()
                    .ToArray();
                foreach (var item in entries)
                {
                    System.IO.FileInfo finfo = new System.IO.FileInfo(item);
                    if (finfo.Attributes.HasFlag(System.IO.FileAttributes.Directory))
                    {
                        //recursively delete directory
                        DeleteDirectory(item, true);
                    }
                    else
                    {
                        System.IO.File.Delete(item);
                    }
                }
            }
        }

        public static void DeleteDirectory(string path, bool recursive)
        {
            if (recursive)
            {
                var subfolders = Directory.GetDirectories(path);
                foreach (var s in subfolders)
                {
                    DeleteDirectory(s, recursive);
                }
            }

            var files = Directory.GetFiles(path);
            foreach (var f in files)
            {
                File.Delete(f);
            }

            Directory.Delete(path);
        }

        /// <summary>
        /// Создать каталог
        /// </summary>
        /// <param name="path">Имя нового каталога</param>
        [ContextMethod("СоздатьКаталог", "CreateDirectory")]
        public void CreateDirectory(string path)
        {
            System.IO.Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Получить текущий каталог
        /// </summary>
        [ContextMethod("ТекущийКаталог", "CurrentDirectory")]
        public string CurrentDirectory()
        {
            return System.IO.Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Установить каталог текущим
        /// </summary>
        /// <param name="path">Имя нового текущего каталога</param>
        [ContextMethod("УстановитьТекущийКаталог", "SetCurrentDirectory")]
        public void SetCurrentDirectory(string path)
        {
            System.IO.Directory.SetCurrentDirectory(path);
        }

        /// <summary>
        /// Получает разделитель пути в соответствии с текущей операционной системой
        /// </summary>
        [ContextMethod("ПолучитьРазделительПути","GetPathSeparator")]
        public string GetPathSeparator()
        {
            return new string(new char[]{Path.DirectorySeparatorChar});
        }

        /// <summary>
        /// Получает маску "все файлы" для текущей операционной системы.
        /// В Windows маска принимает значение "*.*", в nix - "*".
        /// </summary>
        [ContextMethod("ПолучитьМаскуВсеФайлы", "GetAllFilesMask")]
        public string GetAllFilesMask()
        {
            var platform = System.Environment.OSVersion.Platform;
            if (platform == PlatformID.Win32NT || platform == PlatformID.Win32Windows)
                return "*.*";
            else
                return "*";
        }

        /// <summary>
        /// Объединяет компоненты файлового пути, с учетом разделителей, принятых в данной ОС.
        /// При этом корректно, без дублирования, обрабатываются уже существующие разделители пути.
        /// </summary>
        /// <param name="path1">Первая часть пути</param>
        /// <param name="path2">Вторая часть пути</param>
        /// <param name="path3">Третья часть пути (необязательно)</param>
        /// <param name="path4">Четвертая часть пути (необязательно)</param>
        /// <returns>Объединенный путь.</returns>
        [ContextMethod("ОбъединитьПути", "CombinePath")]
        public string CombinePath(string path1, string path2, string path3 = null, string path4 = null)
        {
            if (path3 == null)
                return Path.Combine(path1, path2);
            else if (path4 == null)
                return Path.Combine(path1, path2, path3);
            else
                return Path.Combine(path1, path2, path3, path4);
        }

        public static IAttachableContext CreateInstance()
        {
            return new FileOperations();
        }

    }
}
