/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Localization;

namespace ScriptEngine.Machine.Contexts
{
    public static class BslPredefinedAnnotation
    {
        public static bool NameMatches(this BslAnnotationAttribute attribute, BilingualString names)
        {
            return names.HasName(attribute.Name, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}