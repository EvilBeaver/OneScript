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
    public class AliasedVariableSymbol : IVariableSymbol
    {
        public AliasedVariableSymbol(string name, string alias, Type type)
        {
            Name = name;
            Alias = alias;
            Type = type;
        }

        public AliasedVariableSymbol(string name, string alias)
        {
            Name = name;
            Alias = alias;
            Type = typeof(BslValue);
        }

        public string Name { get; }

        public string Alias { get; }

        public Type Type { get; }
    }
}
