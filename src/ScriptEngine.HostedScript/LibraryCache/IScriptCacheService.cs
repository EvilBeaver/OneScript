/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Execution;

namespace ScriptEngine.HostedScript.LibraryCache
{
    public interface IScriptCacheService
    {
        /// <summary>
        /// Попытаться загрузить предкомпилированный модуль из кэша
        /// </summary>
        /// <param name="location">Путь к исходному файлу сценария</param>
        /// <param name="module">Загруженный модуль, если кэш валиден</param>
        /// <returns>true, если модуль успешно загружен из кэша</returns>
        bool TryLoadFromCache(string location, out IExecutableModule module);

        /// <summary>
        /// Сохранить скомпилированный модуль в кэш
        /// </summary>
        /// <param name="sourceFile">Путь к исходному файлу сценария</param>
        /// <param name="module">Скомпилированный модуль для сохранения</param>
        void SaveToCache(IExecutableModule module);
    }
}