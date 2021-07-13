/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using ScriptEngine.Machine;

namespace ScriptEngine
{
    [Serializable]
    public class VariablesFrame : List<VariableInfo>
    {
        public VariablesFrame()
        {
        }

        public VariablesFrame(IEnumerable<VariableInfo> source) : base(source)
        {
        }
    }
}
