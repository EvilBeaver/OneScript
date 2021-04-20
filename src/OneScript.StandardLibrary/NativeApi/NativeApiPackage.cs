/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Xml;
using Ionic.Zip;
using OneScript.Commons;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Загрузчик для библиотеки внешних компонент, упакованной в zip-файл.
    /// </summary>
    static class NativeApiPackage
    {
        public static bool IsZip(Stream stream)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            // if the first 4 bytes of the array are the ZIP signature then it is compressed data
            const int ZIP_LEAD_BYTES = 0x04034b50;
            return (BitConverter.ToInt32(bytes, 0) == ZIP_LEAD_BYTES);
        }

        public static void Extract(Stream stream, String tempfile)
        {
            stream.Seek(0, 0);
            using (var zip = ZipFile.Read(stream))
            {
                String entryName = GetEntryName(zip);
                ExtractZipEntry(zip, entryName, tempfile);
            }
        }

        private static String GetEntryName(ZipFile zip)
        {
            foreach (var entry in zip.Entries)
            {
                if (string.Equals(entry.FileName, "manifest.xml", StringComparison.OrdinalIgnoreCase))
                {
                    using (var stream = new MemoryStream())
                    {
                        entry.Extract(stream);
                        stream.Seek(0, 0);
                        using (var reader = XmlReader.Create(stream))
                        {
                            while (reader.ReadToFollowing("component"))
                            {
                                var attrOs = reader.GetAttribute("os");
                                var attrArch = reader.GetAttribute("arch");
                                var thisArch = System.Environment.Is64BitOperatingSystem ? "x86_64" : "i386";
                                if (string.Equals(attrOs, "Windows", StringComparison.OrdinalIgnoreCase)
                                    && string.Equals(attrArch, thisArch, StringComparison.OrdinalIgnoreCase))
                                {
                                    return reader.GetAttribute("path");
                                }
                            }
                        }
                    }
                }
            }
            throw new RuntimeException("В библиотеке внешних компонент отсутсует файл требуемой архитектуры");
        }

        private static void ExtractZipEntry(ZipFile zip, String filename, String tempfile)
        {
            foreach (var entry in zip.Entries)
            {
                if (string.Equals(entry.FileName, filename, StringComparison.OrdinalIgnoreCase))
                {
                    using (var stream = File.OpenWrite(tempfile))
                        entry.Extract(stream);
                    return;
                }
            }
            throw new RuntimeException("В библиотеке внешних компонент не обнаружен файл: " + filename);
        }
    }
}
