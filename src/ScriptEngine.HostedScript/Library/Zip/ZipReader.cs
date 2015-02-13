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

        public ZipReader()
        {
        }
	    
        public ZipReader(string filename, string password = null)
        {
            Open(filename, password);
        }

        private void CheckIfOpened()
        {
            if(_zip == null)
                throw new RuntimeException("Архив не открыт");
        }

        [ContextMethod("Открыть","Open")]
        private void Open(string filename, string password)
        {
            _zip = ZipFile.Read(filename);
            _zip.Password = password;
        }


        [ContextMethod("ИзвлечьВсе","ExtractAll")]
        public void ExtractAll(string where, SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths = null)
        {
            CheckIfOpened();
            _zip.FlattenFoldersOnExtract = FlattenPathsOnExtraction(restorePaths);
            _zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
            _zip.ExtractAll(where);
        }

        [ContextMethod("Извлечь", "Extract")]
        public void Extract(ZipFileEntryContext entry, string destination, SelfAwareEnumValue<ZipRestoreFilePathsModeEnum> restorePaths = null, string password = null)
        {
            CheckIfOpened();
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
                CheckIfOpened();

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
        public static ZipReader Construct()
        {
            return new ZipReader();
        }

        [ScriptConstructor(Name="По имени файла")]
        public static ZipReader ConstructByName(IValue filename)
        {
            return new ZipReader(filename.AsString());
        }

        [ScriptConstructor(Name="По имени файла и паролю")]
        public static ZipReader ConstructByNameAndPassword(IValue filename, IValue password)
        {
            return new ZipReader(filename.AsString(), password.AsString());
        }

        public void Dispose()
        {
            _entriesWrapper = null;
            _zip.Dispose();
            _zip = null;
        }
    }
}
