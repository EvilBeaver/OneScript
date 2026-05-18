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
        public const string InMemoryMaxBytesConfigKey = "binaryData.inMemoryMaxBytes";

        /// <summary>
        /// Возвращает лимит в байтах из строки конфигурации; при ошибке — <see cref="BinaryDataConfigurationDefaults.InMemoryMaxBytes"/> и сообщение через <paramref name="logWarning"/>.
        /// </summary>
        public static int ResolveFromConfigString(string rawValue, Action<string> logWarning)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return BinaryDataConfigurationDefaults.InMemoryMaxBytes;

            if (!int.TryParse(rawValue.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var bytes))
            {
                logWarning?.Invoke($"Invalid value for {InMemoryMaxBytesConfigKey}: {rawValue}");
                return BinaryDataConfigurationDefaults.InMemoryMaxBytes;
            }

            if (bytes <= 0 || bytes == int.MaxValue)
            {
                logWarning?.Invoke($"Value for {InMemoryMaxBytesConfigKey} must be between 1 and {int.MaxValue - 1}: {bytes}");
                return BinaryDataConfigurationDefaults.InMemoryMaxBytes;
            }

            return bytes;
        }
    }
}
