/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oscript.DebugServer
{
    internal class DebuggerCommandDescription
    {
        public string Token { get; set; }
        public DebuggerCommands Command { get; set; }
        public string HelpString { get; set; }
        public Action<string[]> Action { get; set; }
    }
}
