/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Localization;

namespace OneScript.Compilation.Binding
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BslOverrideAttribute : Attribute
    {
        public static readonly BilingualString OverrideAttributeName = new BilingualString("Переопределить", "Override");
        
        public static bool AcceptsIdentifier(string identifier)
        {
            return OverrideAttributeName.ContainsString(identifier);
        }
    }
}