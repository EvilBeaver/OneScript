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
        readonly string _givenName;
        string _name;
        string _baseName;
        string _fullName;
        string _path;
        string _extension;

        public FileContext(string name)
        {
            if (String.IsNullOrWhiteSpace (name))
                throw RuntimeException.InvalidArgumentValue ();
            
            _givenName = name;
        }
        
        private string LazyField(ref string value, Func<string, string> algo)
        {
            if (value == null)
                value = algo(_givenName);

            return value;
        }

        [ContextProperty("Имя","Name")]
        public string Name
        {
            get
            {
                return LazyField(ref _name, GetFileNameV8Compatible);
            }
        }

        private string GetFileNameV8Compatible(string arg)
        {
            return System.IO.Path.GetFileName(arg.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
        }

        [ContextProperty("ИмяБезРасширения", "BaseName")]
        public string BaseName
        {
            get
            {
                return LazyField(ref _baseName, System.IO.Path.GetFileNameWithoutExtension);
            }
        }

        [ContextProperty("ПолноеИмя", "FullName")]
        public string FullName
        {
            get
            {
                return LazyField(ref _fullName, System.IO.Path.GetFullPath);
            }
        }

        [ContextProperty("Путь", "Path")]
        public string Path
        {
            get
            {
                return LazyField(ref _path, GetPathWithEndingDelimiter);
            }
        }

        private string GetPathWithEndingDelimiter(string src)
        {
            src = src.Trim ();
            if (src.Length == 1 && src[0] == System.IO.Path.DirectorySeparatorChar)
                return src; // корневой каталог

            var path = System.IO.Path.GetDirectoryName(src.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
            if (path == null)
                return src; // корневой каталог
            
            if (path.Length > 0 && path[path.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                path += System.IO.Path.DirectorySeparatorChar;

            return path;
        }

        [ContextProperty("Расширение", "Extension")]
        public string Extension
        {
            get
            {
                return LazyField(ref _extension, System.IO.Path.GetExtension);
            }
        }

        [ContextMethod("Существует","Exist")]
        public bool Exist()
        {
            try
            {
                File.GetAttributes(FullName);
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }

            return true;
        }

        [ContextMethod("Размер", "Size")]
        public long Size()
        {
            return new FileInfo(FullName).Length;
        }

        [ContextMethod("ПолучитьНевидимость", "GetHidden")]
        public bool GetHidden()
        {
            var attr = File.GetAttributes(FullName);
            return attr.HasFlag(System.IO.FileAttributes.Hidden);
        }

        [ContextMethod("ПолучитьТолькоЧтение", "GetReadOnly")]
        public bool GetReadOnly()
        {
            var attr = File.GetAttributes(FullName);
            return attr.HasFlag(System.IO.FileAttributes.ReadOnly);
        }

        [ContextMethod("ПолучитьВремяИзменения", "GetModificationTime")]
        public DateTime GetModificationTime()
        {
            return File.GetLastWriteTime(FullName);
        }

        [ContextMethod("УстановитьНевидимость", "SetHidden")]
        public void SetHidden(bool value)
        {
            FileSystemInfo entry = new FileInfo(FullName);

            if(value)
                entry.Attributes |= System.IO.FileAttributes.Hidden;
            else
                entry.Attributes &= ~System.IO.FileAttributes.Hidden;
        }

        [ContextMethod("УстановитьТолькоЧтение", "SetReadOnly")]
        public void SetReadOnly(bool value)
        {
            FileSystemInfo entry = new FileInfo(FullName);
            if (value)
                entry.Attributes |= System.IO.FileAttributes.ReadOnly;
            else
                entry.Attributes &= ~System.IO.FileAttributes.ReadOnly;
        }

        [ContextMethod("УстановитьВремяИзменения", "SetModificationTime")]
        public void SetModificationTime(DateTime dateTime)
        {
            FileSystemInfo entry = new FileInfo(FullName);
            entry.LastWriteTime = dateTime;
        }

        [ContextMethod("ЭтоКаталог", "IsDirectory")]
        public bool IsDirectory()
        {
            var attr = File.GetAttributes(FullName);
            return attr.HasFlag(FileAttributes.Directory);
        }

        [ContextMethod("ЭтоФайл", "IsFile")]
        public bool IsFile()
        {
            var attr = File.GetAttributes(FullName);
            return !attr.HasFlag(FileAttributes.Directory);
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue name)
        {
            return new FileContext(name.AsString());
        }
    }
}
