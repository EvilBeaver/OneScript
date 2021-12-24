/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;

namespace OneScript.Runtime.Binding
{
    internal class BslPropertySymbol : IPropertySymbol
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        
        public Type Type => Property.PropertyType;
        
        public PropertyInfo Property { get; set; }
    }
}