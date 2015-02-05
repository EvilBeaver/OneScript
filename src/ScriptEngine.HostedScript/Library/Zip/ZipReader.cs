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
    public class ZipReader : AutoContext<ZipReader>
    {
        ZipFile _zip;

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
    }
}
