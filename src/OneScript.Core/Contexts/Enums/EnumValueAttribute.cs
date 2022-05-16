/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;

namespace OneScript.Contexts.Enums
{
    /// <summary>
    /// Атрибут для задания имени или алиаса для значения перечисления
    /// А также пометки свойства или поля clr-enum, как значения bsl-перечисления
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EnumValueAttribute : Attribute
    {
        public EnumValueAttribute (string name, string alias = null)
        {
            if (!Utils.IsValidIdentifier(name))
                throw new ArgumentException("Name must be a valid identifier");

            if (!string.IsNullOrEmpty(alias) && !Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Alias must be a valid identifier");

            Name = name;
            Alias = alias;
        }

        public string Name { get; }
        public string Alias { get; }
    }
}
