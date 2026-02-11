using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using OneScript.Language.Sources;
using OneScript.Sources;

namespace ScriptEngine.Serialization
{
    public class FileCodeSourceImageProvider : ICodeSourceImageProvider
    {
        public const string ProviderName = "file";
        
        public string ProviderKey => ProviderName;

        public bool CanHandle(ICodeSource source)
        {
            return source is FileCodeSource;
        }

        public CodeSourceImage CreateImage(ICodeSource source)
        {
            var fileSource = (FileCodeSource)source;
            var path = fileSource.Location;
            var info = new FileInfo(path);
            
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["hash"] = ComputeHash(path),
                ["length"] = info.Length.ToString(CultureInfo.InvariantCulture),
                ["lastWriteUtc"] = info.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture)
            };
            
            return new CodeSourceImage
            {
                ProviderKey = ProviderKey,
                Location = path,
                Metadata = metadata
            };
        }

        public bool TryRestore(CodeSourceImage image, out ICodeSource source, out string error)
        {
            source = null;
            error = null;

            if (string.IsNullOrWhiteSpace(image?.Location))
            {
                error = "Source file location is empty";
                return false;
            }

            if (!File.Exists(image.Location))
            {
                error = $"Source file not found: {image.Location}";
                return false;
            }

            var info = new FileInfo(image.Location);
            if (!CheckMetadata(image, info, out error))
                return false;

            source = new FileCodeSource(image.Location);
            return true;
        }

        private static bool CheckMetadata(CodeSourceImage image, FileInfo info, out string error)
        {
            error = null;
            var metadata = image.Metadata;
            if (metadata == null || metadata.Count == 0)
                return true;

            if (TryReadInt64(metadata, "length", out var expectedLength)
                && expectedLength != info.Length)
            {
                error = "Source file length mismatch";
                return false;
            }

            if (TryReadInt64(metadata, "lastWriteUtc", out var expectedTicks)
                && expectedTicks != info.LastWriteTimeUtc.Ticks)
            {
                error = "Source file timestamp mismatch";
                return false;
            }

            if (metadata.TryGetValue("hash", out var expectedHash))
            {
                var actualHash = ComputeHash(info.FullName);
                if (!string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
                {
                    error = "Source file hash mismatch";
                    return false;
                }
            }

            return true;
        }

        private static bool TryReadInt64(IDictionary<string, string> metadata, string key, out long value)
        {
            value = 0;
            return metadata.TryGetValue(key, out var raw)
                   && long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static string ComputeHash(string path)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                var hash = md5.ComputeHash(stream);
                var builder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
