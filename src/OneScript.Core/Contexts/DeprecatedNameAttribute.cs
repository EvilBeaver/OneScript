/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Contexts
{
    /// <summary>
    /// Позволяет объявить терм (имя или алиас), как устаревший.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Enum,
        AllowMultiple = true)]
    public class DeprecatedNameAttribute : Attribute
    {
        public DeprecatedNameAttribute(string name, bool throwOnUse = false)
        {
            Name = name;
            ThrowOnUse = throwOnUse;
        }
        
        public string Name { get; }
        public bool ThrowOnUse { get; }
    }
}