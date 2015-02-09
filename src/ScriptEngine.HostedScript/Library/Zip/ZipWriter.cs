using Ionic.Zip;
using Ionic.Zlib;
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
        private string _filename;

        public ZipWriter()
        {

        }

        [ContextMethod("Открыть", "Open")]
        public void Open(
            string filename, 
            string password = null, 
            string comment = null, 
            SelfAwareEnumValue<ZipCompressionMethodEnum> compressionMethod = null, 
            SelfAwareEnumValue<ZipCompressionLevelEnum> compressionLevel = null,
            SelfAwareEnumValue<ZipEncryptionMethodEnum> encryptionMethod = null)
        {
            _filename = filename;
            _zip = new ZipFile();
            _zip.Password = password;
            _zip.Comment = comment;
            _zip.CompressionMethod = MakeZipCompressionMethod(compressionMethod);
            _zip.CompressionLevel = MakeZipCompressionLevel(compressionLevel);
            _zip.Encryption = MakeZipEncryption(encryptionMethod);
        }

        [ContextMethod("Добавить", "Add")]
        public void Add(string file, SelfAwareEnumValue<ZipStorePathModeEnum> storePathMode, IValue recurseSubdirectories)
        {
            throw new NotImplementedException();
        }

        [ContextMethod("Записать", "Write")]
        public void Write()
        {
            CheckIfOpened();

            _zip.Save(_filename);
            _zip.Dispose();
            _zip = null;
        }

        private CompressionMethod MakeZipCompressionMethod(SelfAwareEnumValue<ZipCompressionMethodEnum> compressionMethod)
        {
            if (compressionMethod == null)
                return CompressionMethod.Deflate;

            var owner = (ZipCompressionMethodEnum)compressionMethod.Owner;
            if (compressionMethod == owner.Deflate)
                return CompressionMethod.Deflate;
            if (compressionMethod == owner.Copy)
                return CompressionMethod.None;

            throw RuntimeException.InvalidArgumentValue();

        }

        private CompressionLevel MakeZipCompressionLevel(SelfAwareEnumValue<ZipCompressionLevelEnum> compressionLevel)
        {
            if (compressionLevel == null)
                return CompressionLevel.Default;

            var owner = (ZipCompressionLevelEnum)compressionLevel.Owner;
            if (compressionLevel == owner.Minimal)
                return CompressionLevel.BestSpeed;
            if (compressionLevel == owner.Optimal)
                return CompressionLevel.Default;
            if (compressionLevel == owner.Maximal)
                return CompressionLevel.BestCompression;

            throw RuntimeException.InvalidArgumentValue();
        }

        private EncryptionAlgorithm MakeZipEncryption(SelfAwareEnumValue<ZipEncryptionMethodEnum> encryptionMethod)
        {
            if (encryptionMethod == null)
                return EncryptionAlgorithm.PkzipWeak;
            
            var enumOwner = (ZipEncryptionMethodEnum)encryptionMethod.Owner;

            if(encryptionMethod == enumOwner.Zip20)
                return EncryptionAlgorithm.PkzipWeak;
            if (encryptionMethod == enumOwner.Aes128)
                return EncryptionAlgorithm.WinZipAes128;
            if (encryptionMethod == enumOwner.Aes256)
                return EncryptionAlgorithm.WinZipAes256;

            throw RuntimeException.InvalidArgumentValue();

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
            var zip = new ZipWriter();
            zip.Open(filename.AsString());
            return zip;
        }

        [ScriptConstructor(Name = "На основании параметров архива")]
        public static ZipWriter ConstructByFileOptions(IValue filename, IValue password, IValue comment, IValue compressionMethod, IValue compressionLevel, IValue encryptionMethod)
        {
            var zip = new ZipWriter();
            zip.Open(filename.AsString(),
                ConvertParam<string>(password),
                ConvertParam<string>(comment),
                ConvertParam<SelfAwareEnumValue<ZipCompressionMethodEnum>>(compressionMethod),
                ConvertParam<SelfAwareEnumValue<ZipCompressionLevelEnum>>(compressionLevel),
                ConvertParam<SelfAwareEnumValue<ZipEncryptionMethodEnum>>(encryptionMethod)
                    );
            return zip;
        }

        private static T ConvertParam<T>(IValue paramSource)
        {
            if (paramSource == null)
                return default(T);

            if (paramSource.DataType == DataType.NotAValidValue)
                return default(T);

            var raw = paramSource.GetRawValue();
            if (typeof(EnumerationValue).IsAssignableFrom(typeof(T)))
            {
                try
                {
                    return (T)raw;
                }
                catch (InvalidCastException)
                {
                    throw RuntimeException.InvalidArgumentType();
                }
            }
            else
                return ContextValuesMarshaller.ConvertParam<T>(raw);
        }
    }
}
