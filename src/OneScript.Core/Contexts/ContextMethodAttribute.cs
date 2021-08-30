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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ContextMethodAttribute : Attribute
    {
        private readonly string _name;
        private readonly string _alias;

        public ContextMethodAttribute(string name, string alias = null)
        {
            if (!Utils.IsValidIdentifier(name))
                throw new ArgumentException("Name must be a valid identifier");

            if (!string.IsNullOrEmpty(alias) && !Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Alias must be a valid identifier");

            _name = name;
            _alias = alias;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetAlias()
        {
            return _alias;
        }

        public string GetAlias(string nativeMethodName)
        {
            if (!string.IsNullOrEmpty(_alias))
            {
                return _alias;
            }
            if (!IsDeprecated)
            {
                return nativeMethodName;
            }
            return null;
        }

        public bool IsDeprecated { get; set; }

        public bool ThrowOnUse { get; set; }
    }
}