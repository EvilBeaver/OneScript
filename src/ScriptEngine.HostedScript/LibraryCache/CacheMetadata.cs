/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace ScriptEngine.HostedScript.LibraryCache
{
    /// <summary>
    /// Метаданные кэшированного модуля
    /// </summary>
    [Serializable]
    public class CacheMetadata
    {
        /// <summary>
        /// Версия формата кэша
        /// </summary>
        public int FormatVersion { get; set; } = 1;

        /// <summary>
        /// Дата модификации исходного файла на момент компиляции
        /// </summary>
        public DateTime SourceModifiedTime { get; set; }

        /// <summary>
        /// Размер исходного файла на момент компиляции
        /// </summary>
        public long SourceSize { get; set; }

        /// <summary>
        /// Путь к исходному файлу
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Время создания кэша
        /// </summary>
        public DateTime CacheCreatedTime { get; set; }

        /// <summary>
        /// Версия среды выполнения, использованной для компиляции
        /// </summary>
        public string RuntimeVersion { get; set; }
    }
}