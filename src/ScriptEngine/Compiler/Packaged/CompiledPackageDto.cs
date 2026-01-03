/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using MessagePack;

namespace ScriptEngine.Compiler.Packaged
{
    /// <summary>
    /// Тип скомпилированного пакета
    /// </summary>
    public enum PackageType : byte
    {
        /// <summary>
        /// Бандл — самодостаточный пакет со всеми зависимостями (.osc)
        /// </summary>
        Bundle = 0,
        
        /// <summary>
        /// Библиотека — переиспользуемый пакет (.oslib)
        /// </summary>
        Library = 1
    }

    /// <summary>
    /// Тип скрипта в пакете
    /// </summary>
    public enum ScriptType : byte
    {
        /// <summary>
        /// Точка входа (главный модуль)
        /// </summary>
        Entry = 0,
        
        /// <summary>
        /// Глобальный модуль библиотеки
        /// </summary>
        Module = 1,
        
        /// <summary>
        /// Класс библиотеки
        /// </summary>
        Class = 2
    }

    /// <summary>
    /// DTO для скомпилированного пакета (бандл или библиотека)
    /// </summary>
    [MessagePackObject]
    public class CompiledPackageDto
    {
        public const int FormatVersion = 1;
        public const string Magic = "OSCP"; // OneScript Compiled Package

        [Key(0)]
        public string MagicHeader { get; set; } = Magic;

        [Key(1)]
        public int Version { get; set; } = FormatVersion;

        [Key(2)]
        public PackageType Type { get; set; }

        /// <summary>
        /// Имя пакета (для библиотек — имя библиотеки)
        /// </summary>
        [Key(3)]
        public string Name { get; set; }

        /// <summary>
        /// Список зависимостей (имена библиотек).
        /// Для бандла — пустой (всё включено).
        /// Для .oslib — список внешних зависимостей.
        /// </summary>
        [Key(4)]
        public List<string> Dependencies { get; set; } = new List<string>();

        /// <summary>
        /// Скрипты пакета (модули, классы, точка входа)
        /// </summary>
        [Key(5)]
        public List<PackagedScriptDto> Scripts { get; set; } = new List<PackagedScriptDto>();
    }

    /// <summary>
    /// DTO для скрипта внутри пакета
    /// </summary>
    [MessagePackObject]
    public class PackagedScriptDto
    {
        /// <summary>
        /// Тип скрипта
        /// </summary>
        [Key(0)]
        public ScriptType Type { get; set; }

        /// <summary>
        /// Символьное имя (для модулей и классов)
        /// </summary>
        [Key(1)]
        public string Symbol { get; set; }

        /// <summary>
        /// Имя библиотеки-владельца (для модулей/классов из библиотек)
        /// </summary>
        [Key(2)]
        public string OwnerLibrary { get; set; }

        /// <summary>
        /// Порядок загрузки (для правильной инициализации)
        /// </summary>
        [Key(3)]
        public int LoadOrder { get; set; }

        /// <summary>
        /// Скомпилированный модуль
        /// </summary>
        [Key(4)]
        public CompiledModuleDto Module { get; set; }
    }
}
