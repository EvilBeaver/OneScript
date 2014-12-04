using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Library;
using System;
using System.Collections.Generic;
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
            return System.IO.Path.GetTempPath();
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
            var entries = System.IO.Directory.EnumerateFileSystemEntries(dir, mask, mode)
                .Select<string, IValue>((x) => new FileContext(x));

            return new ArrayImpl(entries);

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
                    .AsParallel();
                foreach (var item in entries)
                {
                    System.IO.FileInfo finfo = new System.IO.FileInfo(item);
                    if (finfo.Attributes == System.IO.FileAttributes.Directory)
                    {
                        //recursively delete directory
                        System.IO.Directory.Delete(item, true);
                    }
                    else
                    {
                        System.IO.File.Delete(item);
                    }
                }
            }
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

        private static ContextMethodsMapper<FileOperations> _methods = new ContextMethodsMapper<FileOperations>();
        
        public static IAttachableContext CreateInstance()
        {
            return new FileOperations();
        }

    }
}
