/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.Machine;

namespace ScriptEngine.Environment
{
    [Obsolete]
    public struct ScriptModuleHandle
    {
        internal ModuleImage Module { get; set; }
    }

    [Obsolete]
    public struct LoadedModuleHandle
    {
        internal LoadedModule Module { get; set; }
    }

}
