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

namespace ScriptEngine.HostedScript.Library
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
            System.IO.File.Copy(source, destination, true);
        }

        /// <summary>
        /// Перемещает файл из одного расположения в другое.
        /// </summary>
        /// <param name="source">Имя файла-источника</param>
        /// <param name="destination">Имя файла приемника</param>
        [ContextMethod("ПереместитьФайл", "MoveFile")]
        public void MoveFile(string source, string destination)
        {
            System.IO.File.Move(source, destination);
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
        public IRuntimeContextInstance FindFiles(string dir, string mask = null, bool recursive = false)
        {
            if (mask == null)
            {
                recursive = false;
                if (System.Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    mask = "*";
                }
                else
                {
                    mask = "*.*";
                }
            }

            System.IO.SearchOption mode = recursive ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
            try
            {
                var entries = System.IO.Directory.EnumerateFileSystemEntries(dir, mask, mode)
                        .Select<string, IValue>((x) => new FileContext(x));
                return new ArrayImpl(entries);
            }
            catch (DirectoryNotFoundException)
            {
                return new ArrayImpl();
            }

        }

        /// <summary>
        /// Удаление файлов
        /// </summary>
        /// <param name="path">Каталог из которого удаляются файлы, или сам файл.</param>
        /// <param name="mask">Маска файлов. Необязательный параметр. Если указан, то первый параметр трактуется, как каталог.</param>
        [ContextMethod("УдалитьФайлы", "DeleteFiles")]
        public void DeleteFiles(string path, string mask = "")
        {
            if (mask == null)
            {
                var file = new FileContext(path);
                if (file.IsDirectory())
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
        /// Получить текущий каталог
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
