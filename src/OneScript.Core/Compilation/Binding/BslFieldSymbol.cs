/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;

namespace OneScript.Compilation.Binding
{
    internal class BslFieldSymbol : IFieldSymbol
    {
        public BslFieldInfo Field { get; set; }
        
        public string Name => Field.Name;
        
        public string Alias => Field.Alias;

        public Type Type => Field.FieldType;
    }
}