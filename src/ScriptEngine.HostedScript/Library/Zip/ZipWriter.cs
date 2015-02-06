using Ionic.Zip;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Zip
{
    [ContextClass("ЗаписьZipФайла", "ZipFileWriter")]
    public class ZipWriter : AutoContext<ZipWriter>
    {
        private ZipFile _zip;

        public ZipWriter()
        {

        }

        private void CheckIfOpened()
        {
            if (_zip == null)
                throw new RuntimeException("Архив не открыт");
        }

        [ScriptConstructor]
        public static ZipWriter Construct()
        {
            return new ZipWriter();
        }

        [ScriptConstructor(Name="На основании имени файла")]
        public static ZipWriter ConstructByFilename(IValue filename)
        {
            throw new NotImplementedException();
        }

        [ScriptConstructor(Name = "На основании параметров архива")]
        public static ZipWriter ConstructByFileOptions(IValue filename, IValue password, IValue comment, IValue compressionMethod, IValue compressionLevel, IValue encryptionMethod)
        {
            throw new NotImplementedException();
        }
    }
}
