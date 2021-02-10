/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using ScriptEngine.Machine;

namespace ScriptEngine.Hosting
{
    public class EnvironmentProviders : List<Action<MachineEnvironment>>
    {
        public void Invoke(MachineEnvironment env)
        {
            foreach (var provider in this)
            {
                provider(env);
            }
        }
    }
}