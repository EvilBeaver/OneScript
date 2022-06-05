/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;
using OneScript.Contexts.Enums;

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
        [EnumValue("All", "Все")]
        All,

        /// <summary>
        /// Требует наличия только одного из элементов группы
        /// </summary>
        /// <see cref="XmlSchemaChoice"/>
        [EnumValue("Choice", "Выбор")]
        Choice,

        /// <summary>
        /// Требует чтобы элементы следовали в указанной последовательности
        /// </summary>
        /// <see cref="XmlSchemaSequence"/>
        [EnumValue("Sequence", "Последовательность")]
        Sequence
    }
}
