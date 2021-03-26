/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.Native
{
    internal struct VariableSymbol
    {
        public IRuntimeContextInstance Owner;
        
        public PropertyInfo PropertyInfo;
        
        public Type DataType;
    }
}