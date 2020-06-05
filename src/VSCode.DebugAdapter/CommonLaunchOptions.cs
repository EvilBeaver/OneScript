/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace VSCode.DebugAdapter
{
    public abstract class CommonLaunchOptions
    {
        public string RuntimeExecutable { get; set; }

        public int DebugPort { get; set; }

        public string[] RuntimeArgs { get; set; }

        public string Protocol { get; set; }

        public Dictionary<string, string> Env { get; set; }
    }
}