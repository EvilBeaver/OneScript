/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace VSCode.DebugAdapter
{
    internal static class DebugeeFactory
    {
        public static DebugeeProcess CreateProcess(string adapterId, PathHandlingStrategy pathStrategy)
        {
            if (adapterId == "oscript")
            {
                return new ConsoleProcess(pathStrategy);
            }

            if (adapterId == "oscript.web")
            {
                return new ServerProcess(pathStrategy);
            }
            
            throw new ArgumentOutOfRangeException(nameof(adapterId), adapterId, "Unsupported debugger");
        }
    }
}