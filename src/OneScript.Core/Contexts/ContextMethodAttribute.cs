/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
using OneScript.Commons;

namespace OneScript.Contexts
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ContextMethodAttribute : Attribute
    {
        private readonly string _name;
        private readonly string _alias;

        public ContextMethodAttribute(string name, string alias) 
        {
            if (!Utils.IsValidIdentifier(name))
                throw new ArgumentException($"Name '{name}' must be a valid identifier");

            if (!string.IsNullOrEmpty(alias) && !Utils.IsValidIdentifier(alias))
                throw new ArgumentException($"Alias '{alias}' must be a valid identifier");

            _name = name;
            _alias = alias;
        }

        public ContextMethodAttribute(string name, string _ = null, 
            [CallerMemberName] string nativeMethodName = null)
        : this(name, nativeMethodName)
        {
        }

        public string GetName() => _name;

        public string GetAlias() => _alias;

        public bool IsDeprecated { get; set; }

        public bool ThrowOnUse { get; set; }
    }
}