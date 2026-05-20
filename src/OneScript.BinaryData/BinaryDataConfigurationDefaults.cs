/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.BinaryData
{
    /// <summary>
    /// Значения по умолчанию для буферизации двоичных данных в памяти до перехода на временный файл.
    /// </summary>
    public static class BinaryDataConfigurationDefaults
    {
        /// <summary>
        /// Лимит по умолчанию (50 МБ), если в конфигурации не задан ключ binaryData.inMemoryMaxSize.
        /// </summary>
        public const int InMemoryMaxBytes = 1024 * 1024 * 50;
    }
}
