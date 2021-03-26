/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Reflection;
using ScriptEngine.Machine;
using MethodInfo = System.Reflection.MethodInfo;

namespace OneScript.StandardLibrary.Native
{
    internal struct MethodSymbol
    {
        public IRuntimeContextInstance Target;

        public MethodInfo MethodInfo;
    }
}