/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace BslTestsBridge
{
    /// <summary>
    /// Ищет базовый каталог, относительнго которого будут искаться файлы тестов.
    /// </summary>
    public interface IBaseDirectoryLocator
    {
        /// <summary>
        /// Возвращает полный путь к файлу относительно переданного каталога.
        /// </summary>
        string ResolvePath(string relativePath);
    }
}