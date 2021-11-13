/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    /// <summary>
    /// Типы ограничения идентичности.
    /// </summary>
    /// <see cref="XSIdentityConstraintDefinition"/>
    [EnumerationType("КатегорияОграниченияИдентичностиXS", "XSIdentityConstraintCategory")]
    public enum XSIdentityConstraintCategory
    {
        /// <summary>
        /// Ограничение идентичности по ключу
        /// </summary>
        /// <see cref="XmlSchemaKey"/>
        [EnumItem("Ключ", "Key")]
        Key,

        /// <summary>
        /// Ограничение идентичности по ссылке
        /// </summary>
        /// <see cref="XmlSchemaKey"/>
        [EnumItem("СсылкаНаКлюч", "KeyRef")]
        KeyRef,

        /// <summary>
        /// Ограничение идентичности по опредению уникальности
        /// </summary>
        /// <see cref="XmlSchemaUnique"/>
        [EnumItem("Уникальность", "Unique")]
        Unique
    }
}