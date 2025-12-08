/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;

namespace OneScript.Contexts
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ContextPropertyAttribute : Attribute, INameAndAliasProvider
    {
        private readonly string _name;
        private readonly string _alias;

        public ContextPropertyAttribute(string name, string alias = "")
        {
            if (!Utils.IsValidIdentifier(name))
                throw new ArgumentException("Name must be a valid identifier");

            if (!string.IsNullOrEmpty(alias) && !Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Alias must be a valid identifier");

            _name = name;
            _alias = alias;
            CanRead = true;
            CanWrite = true;
        }

        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }

        /// <summary>
        /// Данное свойство не будет обработано генератором документации при обходе типов
        /// </summary>
        public bool SkipForDocumenter { get; set; }

        public string Name => _name;
        public string Alias => _alias;
        
        public bool IsDeprecated { get; set; }

        public bool ThrowOnUse { get; set; }
    }
}