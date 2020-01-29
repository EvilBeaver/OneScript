/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;
using ScriptEngine;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    /// <summary>
    /// Вид группы модели
    /// </summary>
    /// <see cref="XmlSchemaGroupBase"/>
    [EnumerationType("XSCompositor", "ВидГруппыМоделиXS")]
    public enum XSCompositor
    {
        /// <summary>
        /// Требует наличия элементов группы без требования последовательности
        /// </summary>
        /// <see cref="XmlSchemaAll"/>
        [EnumItem("All", "Все")]
        All,

        /// <summary>
        /// Требует наличия только одного из элементов группы
        /// </summary>
        /// <see cref="XmlSchemaChoice"/>
        [EnumItem("Choice", "Выбор")]
        Choice,

        /// <summary>
        /// Требует чтобы элементы следовали в указанной последовательности
        /// </summary>
        /// <see cref="XmlSchemaSequence"/>
        [EnumItem("Sequence", "Последовательность")]
        Sequence
    }
}
