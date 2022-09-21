/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Values;

namespace OneScript.Compilation.Binding
{
    public class LocalVariableSymbol : IVariableSymbol
    {
        public LocalVariableSymbol(string name, Type type)
        {
            Name = name;
            Type = type;
        }
        
        public LocalVariableSymbol(string name)
        {
            Name = name;
            Type = typeof(BslValue);
        }

        public string Name { get; }
        
        public string Alias => null;
        
        public Type Type { get; }
    }
}
