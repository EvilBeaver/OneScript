// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System.Globalization;
using ScriptEngine;
using ScriptEngine.Hosting;

namespace OneScript.StandardLibrary.Binary
{
    /// <summary>
    /// Инкапсулирует логику чтения и хранения настроек двоичных данных
    /// </summary>
    public class BinaryDataOptions : IBinaryDataMemoryLimit
    {
        public const string IN_MEMORY_LIMIT_KEY_NAME = "binaryData.inMemoryMaxSize";
        public const string IN_MEMORY_MAX_MAGIC = "max";
        
        public BinaryDataOptions(KeyValueConfig config)
        {
            var configValue = config[IN_MEMORY_LIMIT_KEY_NAME];
            MaxBytesInMemory = ResolveFromConfigString(configValue);
        }

        public int MaxBytesInMemory { get; }
        
        private static int ResolveFromConfigString(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return BinaryDataConstants.DEFAULT_IN_MEMORY_LIMIT;

            if (rawValue.Trim() == IN_MEMORY_MAX_MAGIC)
                return BinaryDataConstants.SYSTEM_IN_MEMORY_LIMIT;
            
            if (!TryParseByteSize(rawValue.Trim(), out var bytes))
            {
                SystemLogger.Write($"Invalid value for {IN_MEMORY_LIMIT_KEY_NAME}: {rawValue}");
                return BinaryDataConstants.DEFAULT_IN_MEMORY_LIMIT;
            }

            if (bytes <= 0 || bytes >= BinaryDataConstants.SYSTEM_IN_MEMORY_LIMIT)
            {
                SystemLogger.Write($"Value for {IN_MEMORY_LIMIT_KEY_NAME} must be between 1 and {BinaryDataConstants.SYSTEM_IN_MEMORY_LIMIT - 1}: {bytes}");
                return BinaryDataConstants.DEFAULT_IN_MEMORY_LIMIT;
            }

            return (int)bytes;
        }
        
        private static bool TryParseByteSize(string value, out long bytes)
        {
            bytes = 0;

            if (value.Length == 0)
                return false;

            var suffix = value[value.Length - 1];
            long multiplier = 1;
            var numberPart = value;

            const long KILOBYTES = 1024L;
            const long MEGABYTES = KILOBYTES * 1024L;
            const long GIGABYTES = MEGABYTES * 1024L;
            
            switch (suffix)
            {
                case 'k':
                case 'K':
                    multiplier = KILOBYTES;
                    numberPart = value.Substring(0, value.Length - 1).TrimEnd();
                    break;
                case 'm':
                case 'M':
                    multiplier = MEGABYTES;
                    numberPart = value.Substring(0, value.Length - 1).TrimEnd();
                    break;
                case 'g':
                case 'G':
                    multiplier = GIGABYTES;
                    numberPart = value.Substring(0, value.Length - 1).TrimEnd();
                    break;
            }

            if (numberPart.Length == 0)
                return false;
            
            if (!long.TryParse(numberPart, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out var number))
                return false;

            if (number <= 0)
                return false;

            bytes = number * multiplier;
            
            return true;
        }
    }
}