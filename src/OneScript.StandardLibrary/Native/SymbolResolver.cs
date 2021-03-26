/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.StandardLibrary.Native
{
    internal class SymbolResolver
    {
        public void AddVariable(string name, Type type)
        {
            
        }

        public MethodSymbol GetMethod(string name)
        {
            throw new NotImplementedException();
        }

        public VariableSymbol GetVariable(string identifier)
        {
            throw new NotImplementedException();
        }
    }
}
