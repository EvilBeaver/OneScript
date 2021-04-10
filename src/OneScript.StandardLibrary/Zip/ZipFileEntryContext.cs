/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using Ionic.Zip;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Zip
{
    /// <summary>
    /// Описание элемента, находящегося в Zip архиве.
    /// </summary>
    [ContextClass("ЭлементZipФайла", "ZipFileEntry")]
    public class ZipFileEntryContext : AutoContext<ZipFileEntryContext>
    {
        readonly ZipEntry _entry;

        public ZipFileEntryContext(ZipEntry entry)
        {
            _entry = entry;
        }

        public ZipEntry GetZipEntry()
        {
            return _entry;
        }

        [ContextProperty("ВремяИзменения", "Modified")]
        public DateTime Modified
        {
            get
            {
                return _entry.LastModified;
            }
        }

        [ContextProperty("Зашифрован", "Encrypted")]
        public bool Encrypted
        {
            get
            {
                return _entry.UsesEncryption;
            }
        }

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get
            {
                var filename = _entry.FileName;
                return System.IO.Path.GetFileName(filename);
            }
        }

        [ContextProperty("ИмяБезРасширения", "NameWithoutExtension")]
        public string NameWithoutExtension
        {
            get
            {
                var filename = _entry.FileName;
                return System.IO.Path.GetFileNameWithoutExtension(filename);
            }
        }

        [ContextProperty("Невидимый", "Hidden")]
        public bool Hidden
        {
            get
            {
                return _entry.Attributes.HasFlag(System.IO.FileAttributes.Hidden);
            }
        }

        [ContextProperty("ПолноеИмя", "FullName")]
        public string FullName
        {
            get
            {
                return _entry.FileName;
            }
        }

        [ContextProperty("Путь", "Path")]
        public string Path
        {
            get
            {
                var filename = _entry.FileName;
                var dir = System.IO.Path.GetDirectoryName(filename);
                if (dir != String.Empty && !dir.EndsWith(new string(new []{System.IO.Path.DirectorySeparatorChar})))
                    return dir + System.IO.Path.DirectorySeparatorChar;
                else
                    return dir;
            }
        }

        [ContextProperty("РазмерНесжатого", "UncompressedSize")]
        public long UncompressedSize
        {
            get
            {
                return _entry.UncompressedSize;
            }
        }

        [ContextProperty("РазмерСжатого", "CompressedSize")]
        public long CompressedSize
        {
            get
            {
                return _entry.CompressedSize;
            }
        }

        [ContextProperty("Расширение", "Extension")]
        public string Extension
        {
            get
            {
                var filename = _entry.FileName;
                return System.IO.Path.GetExtension(filename);
            }
        }

        [ContextProperty("ТолькоЧтение", "ReadOnly")]
        public bool ReadOnly
        {
            get
            {
                return _entry.Attributes.HasFlag(System.IO.FileAttributes.ReadOnly);
            }
        }
    }
}
