/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using System.Reflection;

namespace BslTestsBridge
{
    public static class Utils
    {
        public static string GetTestRunnerPath() => 
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                "testrunner",
                "testrunner.os"
            );
    }
}