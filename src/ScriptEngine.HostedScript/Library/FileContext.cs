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
    [ContextClass("Файл","File")]
    public class FileContext : AutoContext<FileContext>
    {
        FileSystemInfo _fsEntry;

        public FileContext(string name)
        {
            RefreshEntry(name);
        }

        private void RefreshEntry(string name)
        {
            if (Directory.Exists(name))
                _fsEntry = new DirectoryInfo(name);
            else
                _fsEntry = new FileInfo(name);
        }

        private void RefreshEntry()
        {
            RefreshEntry(_fsEntry.FullName);
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

                if (path.Length > 0 && path[path.Length-1] != System.IO.Path.DirectorySeparatorChar)
                    path += System.IO.Path.DirectorySeparatorChar;

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
            RefreshEntry();

            return _fsEntry.Exists;
        }

        [ContextMethod("Размер", "Size")]
        public long Size()
        {
            if (IsFile())
                return (long)((FileInfo)_fsEntry).Length;
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
            RefreshEntry();
            return _fsEntry is DirectoryInfo;
        }

        [ContextMethod("ЭтоФайл", "IsFile")]
        public bool IsFile()
        {
            RefreshEntry();
            return _fsEntry is FileInfo;
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue name)
        {
            return new FileContext(name.AsString());
        }
    }
}
