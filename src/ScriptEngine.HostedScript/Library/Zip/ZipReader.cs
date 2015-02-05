using Ionic.Zip;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Zip
{
    [ContextClass("ЧтениеZipФайла", "ZipFileReader")]
    public class ZipReader : AutoContext<ZipReader>, IDisposable
    {
        ZipFile _zip;
        ZipFileEntriesCollection _entriesWrapper;

        public ZipReader(string filename)
        {
            _zip = ZipFile.Read(filename);
        }

        [ContextMethod("ИзвлечьВсе","ExtractAll")]
        public void ExtractAll(string where, SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths = null)
        {
            _zip.FlattenFoldersOnExtract = FlattenPathsOnExtraction(restorePaths);
            _zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
            _zip.ExtractAll(where);
        }

        [ContextMethod("Извлечь", "Extract")]
        public void Extract(ZipFileEntryContext entry, string destination, SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths = null, string password = null)
        {
            var realEntry = entry.GetZipEntry();
            _zip.FlattenFoldersOnExtract = FlattenPathsOnExtraction(restorePaths);
            realEntry.Password = password;
            realEntry.Extract(destination);
        }

        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            Dispose();
        }

        [ContextProperty("Элементы", "Elements")]
        public ZipFileEntriesCollection Elements
        {
            get
            {
                if (_entriesWrapper == null)
                    _entriesWrapper = new ZipFileEntriesCollection(_zip.Entries);

                return _entriesWrapper;
            }
        }

        private static bool FlattenPathsOnExtraction(SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths)
        {
            bool flattenFlag = false;
            if (restorePaths != null)
            {
                var zipEnum = (ZipRestoreFilePathsModeEnum)restorePaths.Owner;
                flattenFlag = restorePaths == zipEnum.DoNotRestore;
            }

            return flattenFlag;
        }

        [ScriptConstructor]
        public static ZipReader ConstructByName(IValue filename)
        {
            return new ZipReader(filename.AsString());
        }

        public void Dispose()
        {
            _zip.Dispose();
            _entriesWrapper = null;
        }
    }
}
