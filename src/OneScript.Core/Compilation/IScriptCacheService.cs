/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Execution;
using OneScript.Sources;

namespace OneScript.Compilation
{
    public interface IScriptCacheService
    {
        /// <summary>
        /// Включено ли кэширование
        /// </summary>
        bool CachingEnabled { get; set; }

        /// <summary>
        /// Событие для логирования операций кэша
        /// </summary>
        event System.Action<string> CacheOperationLogged;

        /// <summary>
        /// Установить сериализатор модулей
        /// </summary>
        void SetModuleSerializer(IModuleSerializer serializer);

        /// <summary>
        /// Попытаться загрузить предкомпилированный модуль из кэша
        /// </summary>
        /// <param name="sourceFile">Путь к исходному файлу сценария</param>
        /// <param name="module">Загруженный модуль, если кэш валиден</param>
        /// <returns>true, если модуль успешно загружен из кэша</returns>
        bool TryLoadFromCache(string sourceFile, out IExecutableModule module);

        /// <summary>
        /// Сохранить скомпилированный модуль в кэш
        /// </summary>
        /// <param name="sourceFile">Путь к исходному файлу сценария</param>
        /// <param name="module">Скомпилированный модуль для сохранения</param>
        void SaveToCache(string sourceFile, IExecutableModule module);

        /// <summary>
        /// Очистить кэш для указанного файла
        /// </summary>
        /// <param name="sourceFile">Путь к исходному файлу сценария</param>
        void ClearCache(string sourceFile);

        /// <summary>
        /// Проверить валидность кэша для файла
        /// </summary>
        /// <param name="sourceFile">Путь к исходному файлу сценария</param>
        /// <returns>true, если кэш существует и валиден</returns>
        bool IsCacheValid(string sourceFile);
    }
}