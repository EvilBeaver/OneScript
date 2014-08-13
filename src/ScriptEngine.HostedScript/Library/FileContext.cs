using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("Файл","File")]
    public class FileContext : AutoContext<FileContext>
    {
        FileSystemInfo _fsEntry;

        public FileContext(string name)
        {
            if (Directory.Exists(name))
                _fsEntry = new DirectoryInfo(name);
            else
                _fsEntry = new FileInfo(name);
        }

        [ContextProperty("Имя","Name")]
        public string Name
        {
            get
            {
                return _fsEntry.Name;
            }
        }

        [ContextProperty("ИмяБезРасширения", "BaseName")]
        public string BaseName
        {
            get
            {
                if (IsFile())
                    return System.IO.Path.GetFileNameWithoutExtension(_fsEntry.Name);
                else
                    return _fsEntry.Name;
            }
        }

        [ContextProperty("ПолноеИмя", "FullName")]
        public string FullName
        {
            get
            {
                return _fsEntry.FullName;
            }
        }

        [ContextProperty("Путь", "Path")]
        public string Path
        {
            get
            {
                string path;
                if (IsFile())
                    path = System.IO.Path.GetDirectoryName(_fsEntry.FullName);
                else
                    path = ((DirectoryInfo)_fsEntry).Parent.FullName;

                if (!path.EndsWith("\\")) 
                    path += "\\";

                return path;

            }
        }

        [ContextProperty("Расширение", "Extension")]
        public string Extension
        {
            get
            {
                return _fsEntry.Extension;
            }
        }

        [ContextMethod("Существует","Exists")]
        public bool Exists()
        {
            return _fsEntry.Exists;
        }

        [ContextMethod("Размер", "Size")]
        public int Size()
        {
            if (IsFile())
                return (int)((FileInfo)_fsEntry).Length;
            else
                throw new RuntimeException("Получение размера применимо только к файлам");
        }

        [ContextMethod("ПолучитьНевидимость", "GetHidden")]
        public bool GetHidden()
        {
            var attr = _fsEntry.Attributes;
            return attr.HasFlag(System.IO.FileAttributes.Hidden);
        }

        [ContextMethod("ПолучитьТолькоЧтение", "GetReadOnly")]
        public bool GetReadOnly()
        {
            var attr = _fsEntry.Attributes;
            return attr.HasFlag(System.IO.FileAttributes.ReadOnly);
        }

        [ContextMethod("ПолучитьВремяИзменения", "GetModificationTime")]
        public DateTime GetModificationTime()
        {
            return _fsEntry.LastWriteTime;
        }

        [ContextMethod("УстановитьНевидимость", "SetHidden")]
        public void SetHidden(bool value)
        {
            if(value)
                _fsEntry.Attributes |= System.IO.FileAttributes.Hidden;
            else
                _fsEntry.Attributes &= ~System.IO.FileAttributes.Hidden;
        }

        [ContextMethod("УстановитьТолькоЧтение", "SetReadOnly")]
        public void SetReadOnly(bool value)
        {
            if (value)
                _fsEntry.Attributes |= System.IO.FileAttributes.ReadOnly;
            else
                _fsEntry.Attributes &= ~System.IO.FileAttributes.ReadOnly;
        }

        [ContextMethod("УстановитьВремяИзменения", "SetModificationTime")]
        public void SetModificationTime(DateTime dateTime)
        {
            _fsEntry.LastWriteTime = dateTime;
        }

        [ContextMethod("ЭтоКаталог", "IsDirectory")]
        public bool IsDirectory()
        {
            return _fsEntry is DirectoryInfo;
        }

        [ContextMethod("ЭтоФайл", "IsFile")]
        public bool IsFile()
        {
            return _fsEntry is FileInfo;
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue name)
        {
            return new FileContext(name.AsString());
        }
    }
}
