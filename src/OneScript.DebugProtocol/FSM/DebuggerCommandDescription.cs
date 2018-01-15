/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.DebugProtocol.FSM
{
    public class DebuggerCommandDescription
    {
        public DebuggerCommands Command { get; set; }
        public Action<object[]> Action { get; set; }
    }
}
