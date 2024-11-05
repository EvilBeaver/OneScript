/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Sources;

namespace OneScript.Compilation
{
    public interface ICompileTimeDependencyResolver
    {
        /// <summary>
        /// Загрузить библиотеку для модуля
        /// </summary>
        /// <param name="module">Модуль в котором объявлен импорт</param>
        /// <param name="libraryName">имя библиотеки</param>
        void Resolve(SourceCode module, string libraryName);
    }
}