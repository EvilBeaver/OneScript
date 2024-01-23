﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;

namespace ScriptEngine.Machine.Interfaces
{
    public class IterableBslInterface
    {
        public IterableBslInterface(BslScriptMethodInfo getIteratorMethod, BslScriptMethodInfo getCountMethod)
        {
            GetIteratorMethod = getIteratorMethod;
            GetCountMethod = getCountMethod;
        }
        
        public BslScriptMethodInfo GetIteratorMethod { get; }
        public BslScriptMethodInfo GetCountMethod { get; }
    }
}