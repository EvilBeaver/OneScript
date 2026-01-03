/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using MessagePack;
using OneScript.Compilation.Binding;

namespace ScriptEngine.Compiler.Packaged
{
    /// <summary>
    /// DTO для сериализации привязки символа (переменной или метода)
    /// </summary>
    [MessagePackObject]
    public class SymbolBindingDto
    {
        /// <summary>
        /// Тип привязки: Static, ThisScope, FrameScope
        /// </summary>
        [Key(0)]
        public ScopeBindingKind Kind { get; set; }

        /// <summary>
        /// Номер члена в целевом контексте
        /// </summary>
        [Key(1)]
        public int MemberNumber { get; set; }

        /// <summary>
        /// Индекс области видимости (для FrameScope)
        /// </summary>
        [Key(2)]
        public int ScopeIndex { get; set; }

        /// <summary>
        /// Имя целевого контекста (для Static).
        /// Используется для восстановления ссылки при загрузке.
        /// </summary>
        [Key(3)]
        public string ContextName { get; set; }

        /// <summary>
        /// Имя свойства/метода в контексте (для PropertyBag).
        /// Используется для восстановления MemberNumber при загрузке.
        /// </summary>
        [Key(4)]
        public string MemberName { get; set; }
    }
}
