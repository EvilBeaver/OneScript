/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Compilation
{
    /// <summary>
    /// Информация о внешнем пакете (библиотеке).
    /// </summary>
    public sealed class PackageInfo
    {
        public PackageInfo(string id, string shortName)
        {
            Id = id;
            ShortName = shortName;
        }
        
        /// <summary>
        /// Уникальный идентификатор пакета (обычно путь к каталогу)
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Короткое имя для отображения в сообщениях
        /// </summary>
        public string ShortName { get; }
    }
}
