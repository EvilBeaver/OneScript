/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;

namespace OneScript.BinaryData
{
    /// <summary>
    /// Ключ и разбор параметра конфигурации лимита памяти для двоичных данных (стандартная библиотека).
    /// </summary>
    public static class BinaryDataMemoryLimitConfiguration
    {
        public const string InMemoryMaxSizeConfigKey = "binaryData.inMemoryMaxSize";

        /// <summary>
        /// Возвращает лимит в байтах из строки конфигурации; при ошибке — <see cref="BinaryDataConfigurationDefaults.InMemoryMaxBytes"/> и сообщение через <paramref name="logWarning"/>.
        /// Допускается целое число байт или значение с суффиксом k, m, g (512k, 50m, 1g).
        /// </summary>
        public static int ResolveFromConfigString(string rawValue, Action<string> logWarning)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return BinaryDataConfigurationDefaults.InMemoryMaxBytes;

            if (!TryParseByteSize(rawValue.Trim(), out var bytes))
            {
                logWarning?.Invoke($"Invalid value for {InMemoryMaxSizeConfigKey}: {rawValue}");
                return BinaryDataConfigurationDefaults.InMemoryMaxBytes;
            }

            if (bytes <= 0 || bytes == int.MaxValue)
            {
                logWarning?.Invoke($"Value for {InMemoryMaxSizeConfigKey} must be between 1 and {int.MaxValue - 1}: {bytes}");
                return BinaryDataConfigurationDefaults.InMemoryMaxBytes;
            }

            return bytes;
        }

        private static bool TryParseByteSize(string value, out int bytes)
        {
            bytes = 0;

            if (value.Length == 0)
                return false;

            var suffix = value[value.Length - 1];
            long multiplier = 1;
            var numberPart = value;

            switch (suffix)
            {
                case 'k':
                case 'K':
                    multiplier = 1024L;
                    numberPart = value.Substring(0, value.Length - 1).TrimEnd();
                    break;
                case 'm':
                case 'M':
                    multiplier = 1024L * 1024L;
                    numberPart = value.Substring(0, value.Length - 1).TrimEnd();
                    break;
                case 'g':
                case 'G':
                    multiplier = 1024L * 1024L * 1024L;
                    numberPart = value.Substring(0, value.Length - 1).TrimEnd();
                    break;
            }

            if (numberPart.Length == 0)
                return false;

            if (!long.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                return false;

            if (number <= 0)
                return false;

            var result = number * multiplier;
            if (result <= 0 || result >= int.MaxValue)
                return false;

            bytes = (int)result;
            return true;
        }
    }
}
